namespace Backoffice.Web.ViewModels.Settings;

/// <summary>
/// Represents a section of settings
/// </summary>
public class SettingSectionViewModel
{
    public string Name { get; set; } = string.Empty;
    public List<SettingItemViewModel> Settings { get; set; } = [];
}