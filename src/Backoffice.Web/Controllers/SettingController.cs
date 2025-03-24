using System.Reflection;
using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Constants;
using Backoffice.Domain.Enums;
using Backoffice.Domain.Settings;
using Backoffice.Web.Attributes;
using Backoffice.Web.Filters;
using Backoffice.Web.ViewModels.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.Controllers;

public class SettingController(
    ISettingsService settingsService,
    IMapper mapper) 
    : BaseController
{
    // GET: /Settings
    [Permission(PermissionType.List)]
    public async Task<IActionResult> Index(string? searchTerm = null, int pageNumber = 1)
    {
        // Get all settings, optionally filtered by searchTerm (key prefix)
        var settings = await settingsService.GetAllSettingsAsync(searchTerm);
        
        // Convert to list of view models
        var settingsList = settings.Select(s => new SettingItemViewModel
        {
            Key = s.Key,
            Value = s.Value,
            IsEncrypted = s.Key.Contains("password", StringComparison.OrdinalIgnoreCase) || 
                          s.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                          s.Key.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                          s.Key.Contains("key", StringComparison.OrdinalIgnoreCase) ||
                          s.Key.Contains("token", StringComparison.OrdinalIgnoreCase)
        }).OrderBy(s => s.Key).ToList();
        
        // Group settings by section (using the first part of the key before the first dot)
        var sections = settingsList
            .GroupBy(s => s.Key.Contains('.') ? s.Key.Split('.')[0] : "General")
            .Select(g => new SettingSectionViewModel
            {
                Name = g.Key,
                Settings = g.ToList()
            })
            .OrderBy(s => s.Name)
            .ToList();
            
        // Create view model
        var viewModel = new SettingsListViewModel
        {
            Sections = sections,
            SearchTerm = searchTerm
        };

        return View(viewModel);
    }

    // GET: /Settings/Edit?key=system.sitename
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            NotificationService.AddErrorNotification("Ayar anahtarı belirtilmelidir.");
            return RedirectToAction(nameof(Index));
        }

        // Get the setting value
        var value = await settingsService.GetSettingAsync<string>(key, string.Empty);
        
        // Determine if it should be encrypted based on key name
        var shouldEncrypt = key.Contains("password", StringComparison.OrdinalIgnoreCase) || 
                            key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                            key.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                            key.Contains("key", StringComparison.OrdinalIgnoreCase) ||
                            key.Contains("token", StringComparison.OrdinalIgnoreCase);
                            
        // Create the view model
        var viewModel = new EditSettingViewModel
        {
            Key = key,
            Value = value,
            ShouldEncrypt = shouldEncrypt,
            DataType = DetermineDataType(value)
        };

        return View(viewModel);
    }

    // POST: /Settings/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.SystemConfiguration, ActivityTypes.ConfigChange, true)]
    public async Task<IActionResult> Edit(EditSettingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Save the setting
        var result = await settingsService.SetSettingAsync(
            model.Key, 
            model.Value, 
            model.ShouldEncrypt,
            model.Description);

        if (result)
        {
            NotificationService.AddSuccessNotification($"{model.Key} başarıyla güncellendi.");
            
            // Refresh cache to ensure new settings are immediately available
            await settingsService.RefreshCacheAsync();
            
            return RedirectToAction(nameof(Index));
        }

        NotificationService.AddErrorNotification("Ayar kaydedilirken bir hata oluştu.");
        return View(model);
    }

    // GET: /Settings/Create
    [Permission(PermissionType.Create)]
    public IActionResult Create()
    {
        return View(new CreateSettingViewModel());
    }

    // POST: /Settings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Create)]
    [LogActivity(ActivityCategories.SystemConfiguration, ActivityTypes.ConfigChange, true)]
    public async Task<IActionResult> Create(CreateSettingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if the key already exists
        var existingValue = await settingsService.GetSettingAsync<string>(model.Key, null);
        if (existingValue != null)
        {
            ModelState.AddModelError("Key", "Bu anahtar zaten kullanımda.");
            return View(model);
        }

        // Save the setting
        var result = await settingsService.SetSettingAsync(
            model.Key, 
            model.Value, 
            model.ShouldEncrypt,
            model.Description);

        if (result)
        {
            NotificationService.AddSuccessNotification($"{model.Key} başarıyla oluşturuldu.");
            
            // Refresh cache
            await settingsService.RefreshCacheAsync();
            
            return RedirectToAction(nameof(Index));
        }

        NotificationService.AddErrorNotification("Ayar oluşturulurken bir hata oluştu.");
        return View(model);
    }

    // POST: /Settings/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Delete)]
    [LogActivity(ActivityCategories.SystemConfiguration, ActivityTypes.ConfigChange, true)]
    public async Task<IActionResult> Delete(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            NotificationService.AddErrorNotification("Ayar anahtarı belirtilmelidir.");
            return RedirectToAction(nameof(Index));
        }

        var result = await settingsService.DeleteSettingAsync(key);

        if (result)
        {
            NotificationService.AddSuccessNotification($"{key} başarıyla silindi.");
            
            // Refresh cache
            await settingsService.RefreshCacheAsync();
        }
        else
        {
            NotificationService.AddErrorNotification($"{key} silinirken bir hata oluştu.");
        }

        return RedirectToAction(nameof(Index));
    }
    
    // GET: /Settings/AppSettings
    [Permission(PermissionType.List)]
    public async Task<IActionResult> AppSettings()
    {
        // Build app settings model with commonly used settings
        var appSettings = new AppSettings();

        // Load configuration from database
        await settingsService.BindSettingsAsync(appSettings);
        var model = mapper.Map<AppSettingsViewModel>(appSettings);
        return View(model);
    }

    // POST: /Settings/AppSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.SystemConfiguration, ActivityTypes.ConfigChange, true)]
    public async Task<IActionResult> AppSettings(AppSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get list of properties that should be encrypted
        var secureProps = GetSecureProperties(typeof(AppSettingsViewModel));

        // Map the view model to the domain model
        var appSettings = mapper.Map<AppSettings>(model);
        
        // Save all settings
        var result = await settingsService.SaveSettingsAsync(appSettings, secureProps);

        if (result)
        {
            NotificationService.AddSuccessNotification("Uygulama ayarları başarıyla güncellendi.");
            
            // Refresh cache
            await settingsService.RefreshCacheAsync();
        }
        else
        {
            NotificationService.AddErrorNotification("Ayarlar kaydedilirken bir hata oluştu.");
        }

        return RedirectToAction(nameof(AppSettings));
    }
    
    // Helper method to determine data type from a value
    private static string DetermineDataType(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "string";

        // Try to parse as boolean
        if (bool.TryParse(value, out _))
            return "boolean";

        // Try to parse as integer
        if (int.TryParse(value, out _))
            return "number";

        // Try to parse as decimal
        if (decimal.TryParse(value, out _))
            return "number";

        // Try to parse as date
        if (DateTime.TryParse(value, out _))
            return "datetime";

        // If it starts with { or [, it's likely JSON
        if ((value.StartsWith("{") && value.EndsWith("}")) || 
            (value.StartsWith("[") && value.EndsWith("]")))
            return "json";

        // Default to string
        return "string";
    }

    // Helper method to get property names that should be encrypted
    private static string[] GetSecureProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.Contains("Password") || 
                        p.Name.Contains("Secret") || 
                        p.Name.Contains("Key") ||
                        p.Name.Contains("Token") ||
                        p.GetCustomAttribute<EncryptAttribute>() != null)
            .Select(p => p.Name)
            .ToArray();
    }
}