using Backoffice.Domain.Entities.Common;

namespace Backoffice.Domain.Entities.Menu;

/// <summary>
/// Represents a menu item in the application
/// </summary>
public class MenuItem : AuditableEntity<int>
{
    /// <summary>
    /// Name of the menu item to be displayed
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Icon class for the menu item (e.g., "fas fa-home")
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// Controller name for the route
    /// </summary>
    public string? Controller { get; set; }
    
    /// <summary>
    /// Action name for the route
    /// </summary>
    public string? Action { get; set; }
    
    /// <summary>
    /// External URL (used if Controller and Action are null)
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Order of the menu item in its parent menu
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Parent menu item ID (null for top-level items)
    /// </summary>
    public int? ParentId { get; set; }
    
    /// <summary>
    /// Required permission code to view this menu item
    /// If null, all authenticated users can see this item
    /// </summary>
    public string? RequiredPermissionCode { get; set; }
    
    /// <summary>
    /// Whether this is a section header (not clickable)
    /// </summary>
    public bool IsSectionHeader { get; set; }
    
    /// <summary>
    /// Whether this menu item is active/visible
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Navigation property for parent menu item
    /// </summary>
    public virtual MenuItem? Parent { get; set; }
    
    /// <summary>
    /// Navigation property for child menu items
    /// </summary>
    public virtual ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
}