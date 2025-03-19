namespace Backoffice.Application.DTOs.Menu;

public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentId { get; set; }
    public string? RequiredPermissionCode { get; set; }
    public bool IsSectionHeader { get; set; }
    public bool IsActive { get; set; }
    public List<MenuItemDto> Children { get; set; } = new();
    public bool IsVisible { get; set; } // Used for UI to determine if the user has permission to see this item
}