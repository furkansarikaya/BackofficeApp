using Backoffice.Domain.Enums;

namespace Backoffice.Application.DTOs.Logging;

/// <summary>
/// Log listesi için filtre DTO
/// </summary>
public class LogFilterDto
{
    /// <summary>
    /// Filtre için log seviyesi
    /// </summary>
    public LogLevel? Level { get; set; }
    
    /// <summary>
    /// Filtre için log kategorisi
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Filtre için log metni (mesaj, exception içinde arama)
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Başlangıç tarihi (UTC)
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Bitiş tarihi (UTC)
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public string? UserId { get; set; }
}