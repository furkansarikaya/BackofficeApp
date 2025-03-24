namespace Backoffice.Web.ViewModels.Logging;

/// <summary>
/// Log istatistikleri için view model
/// </summary>
public class LogStatisticsViewModel
{
    /// <summary>
    /// Kategori başına log sayımları
    /// </summary>
    public Dictionary<string, Dictionary<Domain.Enums.LogLevel, int>> StatisticsByCategory { get; set; } = new();
    
    /// <summary>
    /// Log seviyesine göre toplam sayımlar
    /// </summary>
    public Dictionary<Domain.Enums.LogLevel, int> TotalsByLevel { get; set; } = new();
    
    /// <summary>
    /// Toplam log sayısı
    /// </summary>
    public int TotalLogCount { get; set; }
    
    /// <summary>
    /// Son 24 saat için log sayısı
    /// </summary>
    public int LogCountLast24Hours { get; set; }
    
    /// <summary>
    /// Son 7 gün için log sayısı
    /// </summary>
    public int LogCountLast7Days { get; set; }
    
    /// <summary>
    /// İstatistikler için tarih aralığı
    /// </summary>
    public DateTime FromDate { get; set; }
    
    /// <summary>
    /// İstatistikler için bitiş tarihi
    /// </summary>
    public DateTime ToDate { get; set; }
}