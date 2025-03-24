namespace Backoffice.Web.ViewModels.Settings;

/// <summary>
/// List view model for settings display
/// </summary>
public class SettingsListViewModel
{
    public List<SettingSectionViewModel> Sections { get; set; } = [];
    public string? SearchTerm { get; set; }
}