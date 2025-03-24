using Backoffice.Domain.Settings;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Service for managing application settings stored in the database
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by its key, converting to the specified type
    /// </summary>
    /// <typeparam name="T">The type to convert the setting value to</typeparam>
    /// <param name="key">The setting key in dot notation</param>
    /// <param name="defaultValue">Default value if setting not found</param>
    /// <returns>The setting value converted to the specified type</returns>
    Task<T> GetSettingAsync<T>(string key, T defaultValue = default);
    
    /// <summary>
    /// Sets a setting value for the specified key
    /// </summary>
    /// <typeparam name="T">The type of the value being set</typeparam>
    /// <param name="key">The setting key in dot notation</param>
    /// <param name="value">The value to set</param>
    /// <param name="encrypt">Whether to encrypt the value</param>
    /// <param name="description">Optional description for the setting</param>
    /// <returns>True if successful</returns>
    Task<bool> SetSettingAsync<T>(string key, T value, bool encrypt = false, string description = null);
    
    /// <summary>
    /// Gets all settings with optional filtering
    /// </summary>
    /// <param name="keyPrefix">Optional prefix to filter settings by</param>
    /// <returns>Dictionary of settings with their keys and values</returns>
    Task<Dictionary<string, string>> GetAllSettingsAsync(string keyPrefix = null);
    
    /// <summary>
    /// Populates a settings object from database values
    /// </summary>
    /// <typeparam name="T">Type of settings object</typeparam>
    /// <param name="settings">Settings object instance to populate</param>
    /// <param name="keyPrefix">Optional prefix override (defaults to class name)</param>
    /// <returns>The populated settings object</returns>
    Task<T> BindSettingsAsync<T>(T settings, string keyPrefix = null) where T : ISettings, new();
    
    /// <summary>
    /// Saves a settings object to the database
    /// </summary>
    /// <typeparam name="T">Type of settings object</typeparam>
    /// <param name="settings">Settings object to save</param>
    /// <param name="encrypt">Properties to encrypt (property names)</param>
    /// <param name="keyPrefix">Optional prefix override (defaults to class name)</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveSettingsAsync<T>(T settings, string[] encryptedProps = null, string keyPrefix = null) where T : ISettings;
    
    /// <summary>
    /// Deletes a setting by its key
    /// </summary>
    /// <param name="key">The setting key to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteSettingAsync(string key);
    
    /// <summary>
    /// Deletes all settings with a specific prefix
    /// </summary>
    /// <param name="keyPrefix">The prefix of keys to delete</param>
    /// <returns>Number of settings deleted</returns>
    Task<int> DeleteSettingsAsync(string keyPrefix);
    
    /// <summary>
    /// Refreshes cached settings
    /// </summary>
    Task RefreshCacheAsync();
}