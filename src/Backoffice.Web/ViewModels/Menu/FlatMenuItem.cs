namespace Backoffice.Web.ViewModels.Menu;

public class FlatMenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public string? RequiredPermissionCode { get; set; }
    public bool IsSectionHeader { get; set; }
    public bool IsActive { get; set; }
    public int Level { get; set; }
}