using System.ComponentModel.DataAnnotations;

namespace Backoffice.Web.ViewModels.Auditing;

public class ActivityLogFilterViewModel
{
    [Display(Name = "Kullanıcı")]
    public string? UserId { get; set; }
    
    [Display(Name = "Kategori")]
    public string? Category { get; set; }
    
    [Display(Name = "İşlem Tipi")]
    public string? ActivityType { get; set; }
    
    [Display(Name = "Varlık Tipi")]
    public string? EntityType { get; set; }
    
    [Display(Name = "Varlık ID")]
    public string? EntityId { get; set; }
    
    [Display(Name = "Arama")]
    public string? SearchTerm { get; set; }
    
    [Display(Name = "Başlangıç Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }
    
    [Display(Name = "Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }
}