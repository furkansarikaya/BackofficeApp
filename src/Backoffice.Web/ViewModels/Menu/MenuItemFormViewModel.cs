using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Backoffice.Web.ViewModels.Menu;

public class MenuItemFormViewModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "Menü adı zorunludur.")]
    [Display(Name = "Menü Adı")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "İkon (FontAwesome sınıfı)")]
    public string? Icon { get; set; }
    
    [Display(Name = "Controller")]
    public string? Controller { get; set; }
    
    [Display(Name = "Action")]
    public string? Action { get; set; }
    
    [Display(Name = "URL (Controller/Action yerine URL kullanılacaksa)")]
    public string? Url { get; set; }
    
    [Display(Name = "Sıralama")]
    [Range(1, 1000, ErrorMessage = "Sıralama 1-1000 arasında olmalıdır.")]
    public int DisplayOrder { get; set; } = 100;
    
    [Display(Name = "Üst Menü")]
    public int? ParentId { get; set; }
    
    [Display(Name = "Gerekli İzin Kodu")]
    public string? RequiredPermissionCode { get; set; }
    
    [Display(Name = "Bölüm Başlığı")]
    public bool IsSectionHeader { get; set; }
    
    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
    
    // For dropdown lists
    public List<SelectListItem> ParentMenuItems { get; set; } = new();
    public List<SelectListItem> PermissionCodes { get; set; } = new();
}