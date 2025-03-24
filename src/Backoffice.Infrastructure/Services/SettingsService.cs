using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Backoffice.Infrastructure.Services;

public class SettingsService(
    IUnitOfWork unitOfWork,
    ICryptographyService cryptographyService,
    IMemoryCache memoryCache,
    IDbLoggerService logger)
    : ISettingsService
{
    private const string CacheKeyPrefix = "AppSetting_";
    private const string AllSettingsCacheKey = "AllSettings";

    public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default)
    {
        key = NormalizeKey(key);
        
        try
        {
            // Try to get from cache first
            if (memoryCache.TryGetValue($"{CacheKeyPrefix}{key}", out string cachedValue))
            {
                return ConvertValue<T>(cachedValue);
            }
            
            // Not in cache, get from database
            var repository = unitOfWork.Repository<Setting, int>();
            var setting = await repository.GetWithIncludeStringAsync(
                predicate: s => s.Key == key);
                
            var settingEntity = setting.FirstOrDefault();
            
            if (settingEntity == null)
            {
                await logger.LogInformationAsync($"Setting {key} not found, using default value: {defaultValue}", "SettingsService");
                return defaultValue;
            }
            
            var value = settingEntity.Value;
            
            // Decrypt if necessary
            if (settingEntity.IsEncrypted)
            {
                value = cryptographyService.Decrypt(value);
            }
            
            // Convert value to requested type
            var result = ConvertValue<T>(value);
            
            // Cache the value
            memoryCache.Set($"{CacheKeyPrefix}{key}", value, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error getting setting {key}: {ex.Message}", "SettingsService");
            return defaultValue;
        }
    }
    
    public async Task<bool> SetSettingAsync<T>(string key, T value, bool encrypt = false, string description = null)
    {
        key = NormalizeKey(key);
        
        try
        {
            var repository = unitOfWork.Repository<Setting, int>();
            
            // Check if setting already exists
            var existingSettings = await repository.GetWithIncludeStringAsync(
                predicate: s => s.Key == key);
                
            var setting = existingSettings.FirstOrDefault();
            
            string stringValue = ConvertToString(value);
            
            // Encrypt if requested
            string storedValue = encrypt ? cryptographyService.Encrypt(stringValue) : stringValue;
            
            if (setting == null)
            {
                // Create new setting
                setting = new Setting
                {
                    Key = key,
                    Value = storedValue,
                    Description = description,
                    IsEncrypted = encrypt,
                    DataType = typeof(T).Name.ToLowerInvariant()
                };
                
                await repository.AddAsync(setting);
            }
            else
            {
                // Update existing setting
                setting.Value = storedValue;
                
                if (description != null)
                {
                    setting.Description = description;
                }
                
                setting.IsEncrypted = encrypt;
                
                await repository.UpdateAsync(setting);
            }
            
            await unitOfWork.SaveChangesAsync();
            
            // Update cache
            memoryCache.Set($"{CacheKeyPrefix}{key}", stringValue, TimeSpan.FromMinutes(30));
            memoryCache.Remove(AllSettingsCacheKey);
            
            return true;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error setting {key} to {value}: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }
    
    public async Task<Dictionary<string, string>> GetAllSettingsAsync(string keyPrefix = null)
    {
        try
        {
            // Check if all settings are in cache
            if (memoryCache.TryGetValue(AllSettingsCacheKey, out Dictionary<string, string> cachedSettings))
            {
                if (string.IsNullOrEmpty(keyPrefix))
                {
                    return cachedSettings;
                }
                
                return cachedSettings
                    .Where(kvp => kvp.Key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            
            // Get all settings from database
            var repository = unitOfWork.Repository<Setting, int>();
            var query = repository.GetQueryable();
            
            if (!string.IsNullOrEmpty(keyPrefix))
            {
                query = query.Where(s => s.Key.StartsWith(keyPrefix));
            }
            
            var settings = await query.ToListAsync();
            
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var setting in settings)
            {
                var value = setting.Value;
                
                // Decrypt if necessary
                if (setting.IsEncrypted)
                {
                    value = cryptographyService.Decrypt(value);
                }
                
                result[setting.Key] = value;
                
                // Cache individual setting
                memoryCache.Set($"{CacheKeyPrefix}{setting.Key}", value, TimeSpan.FromMinutes(30));
            }
            
            // Cache all settings
            memoryCache.Set(AllSettingsCacheKey, result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error getting all settings from {keyPrefix}", "SettingsService", ex);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
    
    public async Task<T> BindSettingsAsync<T>(T settings, string keyPrefix = null) where T : class, new()
    {
        if (settings == null)
        {
            settings = new T();
        }
        
        var type = typeof(T);
        keyPrefix ??= NormalizeKey(type.Name);
        
        // Get all settings with this prefix
        var allSettings = await GetAllSettingsAsync(keyPrefix);
        
        // Get all properties of the settings object
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);
        
        foreach (var property in properties)
        {
            var key = $"{keyPrefix}.{property.Name}".ToLowerInvariant();

            if (!allSettings.TryGetValue(key, out var value)) continue;
            try
            {
                // Convert value to property type and set
                var convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(settings, convertedValue);
            }
            catch (Exception ex)
            {
                await logger.LogWarningAsync($"Failed to convert value '{value}' for property {property.Name}: {ex.Message}", "SettingsService");
            }
        }
        
        return settings;
    }
    
    public async Task<bool> SaveSettingsAsync<T>(T settings, string[] encryptedProps = null, string keyPrefix = null) where T : class
    {
        if (settings == null)
        {
            return false;
        }
        
        var type = typeof(T);
        keyPrefix ??= NormalizeKey(type.Name);
        
        // Get all properties of the settings object
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);
            
        // Default encryption list is empty if not specified
        encryptedProps ??= [];
        
        try
        {
            foreach (var property in properties)
            {
                var key = $"{keyPrefix}.{property.Name}".ToLowerInvariant();
                var value = property.GetValue(settings);

                if (value == null) continue;
                var encrypt = encryptedProps.Contains(property.Name, StringComparer.OrdinalIgnoreCase);
                    
                // Get description from attribute if available
                string description = null;
                var descAttr = property.GetCustomAttribute<DescriptionAttribute>();
                if (descAttr != null)
                {
                    description = descAttr.Description;
                }
                    
                // Use SetSetting to handle type conversion and saving
                await SetSettingAsync(key, value, encrypt, description);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error saving settings object {type.Name}: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }
    
    public async Task<bool> DeleteSettingAsync(string key)
    {
        key = NormalizeKey(key);
        
        try
        {
            var repository = unitOfWork.Repository<Setting, int>();
            
            var settingsToDelete = await repository.GetWithIncludeStringAsync(
                predicate: s => s.Key == key);
                
            var setting = settingsToDelete.FirstOrDefault();
            
            if (setting == null)
            {
                return false;
            }
            
            await repository.DeleteAsync(setting);
            await unitOfWork.SaveChangesAsync();
            
            // Remove from cache
            memoryCache.Remove($"{CacheKeyPrefix}{key}");
            memoryCache.Remove(AllSettingsCacheKey);
            
            return true;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error deleting setting {key}", "SettingsService", ex);
            return false;
        }
    }
    
    public async Task<int> DeleteSettingsAsync(string keyPrefix)
    {
        keyPrefix = NormalizeKey(keyPrefix);
        
        try
        {
            var repository = unitOfWork.Repository<Setting, int>();
            
            var settingsToDelete = await repository.GetWithIncludeStringAsync(
                predicate: s => s.Key.StartsWith(keyPrefix));
                
            if (!settingsToDelete.Any())
            {
                return 0;
            }
            
            var count = 0;
            foreach (var setting in settingsToDelete)
            {
                await repository.DeleteAsync(setting);
                count++;
                
                // Remove individual items from cache
                memoryCache.Remove($"{CacheKeyPrefix}{setting.Key}");
            }
            
            await unitOfWork.SaveChangesAsync();
            
            // Remove all settings cache
            memoryCache.Remove(AllSettingsCacheKey);
            
            return count;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error deleting setting {keyPrefix}", "SettingsService", ex);
            return 0;
        }
    }
    
    public async Task RefreshCacheAsync()
    {
        // Remove all cached settings
        memoryCache.Remove(AllSettingsCacheKey);
        
        // Get all keys from database and refresh cache
        var repository = unitOfWork.Repository<Setting, int>();
        var settings = await repository.GetAllAsync();
        
        foreach (var setting in settings)
        {
            memoryCache.Remove($"{CacheKeyPrefix}{setting.Key}");
        }
        
        // Re-populate cache
        await GetAllSettingsAsync();
    }
    
    #region Helper Methods
    
    private static string NormalizeKey(string key)
    {
        return key.ToLowerInvariant();
    }
    
    private static T ConvertValue<T>(string value)
    {
        return (T)ConvertValue(value, typeof(T));
    }
    
    private static object ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
        
        if (targetType == typeof(string))
        {
            return value;
        }
        
        if (targetType == typeof(int) || targetType == typeof(int?))
        {
            return int.TryParse(value, out var intValue) ? intValue : default(int);
        }
        
        if (targetType == typeof(long) || targetType == typeof(long?))
        {
            return long.TryParse(value, out var longValue) ? longValue : default(long);
        }
        
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            return bool.TryParse(value, out var boolValue) ? boolValue : default(bool);
        }
        
        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
        {
            return decimal.TryParse(value, out var decimalValue) ? decimalValue : default(decimal);
        }
        
        if (targetType == typeof(double) || targetType == typeof(double?))
        {
            return double.TryParse(value, out var doubleValue) ? doubleValue : default(double);
        }
        
        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
        {
            return DateTime.TryParse(value, out var dateValue) ? dateValue : default(DateTime);
        }
        
        if (targetType == typeof(Guid) || targetType == typeof(Guid?))
        {
            return Guid.TryParse(value, out var guidValue) ? guidValue : default(Guid);
        }
        
        if (targetType.IsEnum)
        {
            return Enum.TryParse(targetType, value, true, out var enumValue) ? enumValue : Activator.CreateInstance(targetType);
        }
        
        // For complex types, try to deserialize from JSON
        try
        {
            return JsonSerializer.Deserialize(value, targetType);
        }
        catch
        {
            // If all else fails, try the type converter
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(value);
            }
            
            // If we get here, we can't convert
            throw new InvalidOperationException($"Cannot convert value '{value}' to type {targetType.Name}");
        }
    }
    
    private static string ConvertToString<T>(T value)
    {
        if (value == null)
        {
            return string.Empty;
        }
        
        var type = typeof(T);
        
        if (type == typeof(string))
        {
            return value.ToString();
        }
        
        if (type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || 
            type == typeof(Guid) || type.IsEnum || type == typeof(string))
        {
            return value.ToString();
        }
        
        // For complex types, serialize to JSON
        return JsonSerializer.Serialize(value);
    }
    
    #endregion
}