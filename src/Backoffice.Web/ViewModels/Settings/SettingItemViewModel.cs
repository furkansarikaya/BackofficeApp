namespace Backoffice.Web.ViewModels.Settings;

/// <summary>
/// Individual setting item
/// </summary>
public class SettingItemViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
}