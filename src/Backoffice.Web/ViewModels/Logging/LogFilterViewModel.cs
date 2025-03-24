using System.ComponentModel.DataAnnotations;

namespace Backoffice.Web.ViewModels.Logging;

/// <summary>
/// Log filtreleme için view model
/// </summary>
public class LogFilterViewModel
{
    /// <summary>
    /// Log seviyesi
    /// </summary>
    [Display(Name = "Seviye")]
    public Domain.Enums.LogLevel? Level { get; set; }
    
    /// <summary>
    /// Log kategorisi
    /// </summary>
    [Display(Name = "Kategori")]
    public string? Category { get; set; }
    
    /// <summary>
    /// Arama terimi
    /// </summary>
    [Display(Name = "Arama")]
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    [Display(Name = "Başlangıç Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Bitiş tarihi
    /// </summary>
    [Display(Name = "Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// Kullanıcı ID
    /// </summary>
    [Display(Name = "Kullanıcı")]
    public string? UserId { get; set; }
}