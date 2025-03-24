using System.ComponentModel.DataAnnotations;

namespace Backoffice.Web.ViewModels.Settings;

/// <summary>
/// Model for editing a setting
/// </summary>
public class EditSettingViewModel
{
    [Required(ErrorMessage = "Ayar anahtarı zorunludur.")]
    [Display(Name = "Anahtar")]
    public string Key { get; set; } = string.Empty;
    
    [Display(Name = "Değer")]
    public string Value { get; set; } = string.Empty;
    
    [Display(Name = "Şifrelenmiş")]
    public bool ShouldEncrypt { get; set; }
    
    [Display(Name = "Veri Tipi")]
    public string DataType { get; set; } = "string";
    
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}