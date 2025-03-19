using System.ComponentModel.DataAnnotations;
using Backoffice.Domain.Enums;

namespace Backoffice.Web.ViewModels.Security;

public class IpFilterFormViewModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "IP adresi zorunludur.")]
    [Display(Name = "IP Adresi")]
    [RegularExpression(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\/([0-9]|[1-2][0-9]|3[0-2]))?$", 
        ErrorMessage = "Geçersiz IP adresi formatı. Örnek: 192.168.1.1 veya 192.168.1.0/24")]
    public string IpAddress { get; set; } = string.Empty;
    
    [Display(Name = "Açıklama")]
    [StringLength(255, ErrorMessage = "Açıklama en fazla 255 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Filtre tipi zorunludur.")]
    [Display(Name = "Filtre Tipi")]
    public FilterType FilterType { get; set; }
    
    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}