using System.ComponentModel.DataAnnotations;

namespace Backoffice.Application.DTOs.Menu;

public class CreateUpdateMenuItemDto
{
    public int? Id { get; set; } // Null for create, non-null for update

    [Required(ErrorMessage = "Menü adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Menü adı en fazla 100 karakter olmalıdır.")]
    [Display(Name = "Menü Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "İkon sınıfı en fazla 50 karakter olmalıdır.")]
    [Display(Name = "İkon Sınıfı")]
    public string? Icon { get; set; }

    [StringLength(100, ErrorMessage = "Controller adı en fazla 100 karakter olmalıdır.")]
    [Display(Name = "Controller")]
    public string? Controller { get; set; }

    [StringLength(100, ErrorMessage = "Action adı en fazla 100 karakter olmalıdır.")]
    [Display(Name = "Action")]
    public string? Action { get; set; }

    [StringLength(500, ErrorMessage = "URL en fazla 500 karakter olmalıdır.")]
    [Display(Name = "URL")]
    public string? Url { get; set; }

    [Display(Name = "Görüntüleme Sırası")]
    [Range(1, 1000, ErrorMessage = "Görüntüleme sırası 1-1000 arasında olmalıdır.")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Üst Menü")]
    public int? ParentId { get; set; }

    [StringLength(100, ErrorMessage = "İzin kodu en fazla 100 karakter olmalıdır.")]
    [Display(Name = "Gerekli İzin Kodu")]
    public string? RequiredPermissionCode { get; set; }

    [Display(Name = "Bölüm Başlığı mı?")]
    public bool IsSectionHeader { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}