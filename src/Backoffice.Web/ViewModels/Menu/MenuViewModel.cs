namespace Backoffice.Web.ViewModels.Menu;

public class MenuViewModel
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
    public bool IsActive { get; set; } = true;
    public List<MenuViewModel> Children { get; set; } = new();
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// Menü öğesinin mevcut sayfa olup olmadığını belirler
    /// </summary>
    public bool IsCurrentPage { get; set; }
    
    /// <summary>
    /// Alt menülerin gösterilip gösterilmeyeceğini belirler
    /// </summary>
    public bool IsExpanded { get; set; }
    
    /// <summary>
    /// Bu menü öğesinin URL'sini oluşturur
    /// </summary>
    public string GetUrl()
    {
        if (!string.IsNullOrEmpty(Url))
        {
            return Url;
        }
        
        if (!string.IsNullOrEmpty(Controller) && !string.IsNullOrEmpty(Action))
        {
            return $"/{Controller}/{Action}";
        }
        
        return "#";
    }
    
    /// <summary>
    /// Alt menüsü olan bir öğe mi?
    /// </summary>
    public bool HasChildren => Children.Count > 0;
    
    /// <summary>
    /// Bu menü öğesinin benzersiz ID'si (HTML id özelliği için)
    /// </summary>
    public string HtmlId => $"menu-item-{Id}";
    
    /// <summary>
    /// Bu menü öğesinin alt menü listesinin benzersiz ID'si
    /// </summary>
    public string ChildrenHtmlId => $"submenu-{Id}";
}