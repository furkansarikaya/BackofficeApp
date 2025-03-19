using System.ComponentModel.DataAnnotations;

namespace Backoffice.Web.ViewModels.Role;

public class RoleFormViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Rol adı zorunludur.")]
    [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olmalıdır.")]
    [Display(Name = "Rol Adı")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olmalıdır.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}