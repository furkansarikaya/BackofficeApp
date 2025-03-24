using System.ComponentModel.DataAnnotations;

namespace Backoffice.Web.ViewModels.Settings;

/// <summary>
/// Model for creating a new setting
/// </summary>
public class CreateSettingViewModel
{
    [Required(ErrorMessage = "Ayar anahtarı zorunludur.")]
    [Display(Name = "Anahtar")]
    public string Key { get; set; } = string.Empty;
    
    [Display(Name = "Değer")]
    public string Value { get; set; } = string.Empty;
    
    [Display(Name = "Şifrelenmiş")]
    public bool ShouldEncrypt { get; set; }
    
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}