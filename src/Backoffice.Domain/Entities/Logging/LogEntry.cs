using Backoffice.Domain.Entities.Common;
using Backoffice.Domain.Enums;

namespace Backoffice.Domain.Entities.Logging;

/// <summary>
/// Sistem günlük kayıtları için varlık sınıfı
/// </summary>
public class LogEntry : BaseEntity<long>
{
    /// <summary>
    /// Log seviyesi (Information, Warning, Error, Critical, etc.)
    /// </summary>
    public LogLevel Level { get; set; }
    
    /// <summary>
    /// Log mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Log kaynağı/kategorisi (örn: "System", "Database", "Authentication", etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// İlgili exception mesajı (varsa)
    /// </summary>
    public string? Exception { get; set; }
    
    /// <summary>
    /// İlgili exception stack trace (varsa)
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// İşlemi yapan kullanıcı ID (varsa)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// İşlemi yapan kullanıcı adı (varsa)
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// İstek URL'i (varsa)
    /// </summary>
    public string? RequestPath { get; set; }
    
    /// <summary>
    /// İstek HTTP metodu (varsa)
    /// </summary>
    public string? RequestMethod { get; set; }
    
    /// <summary>
    /// İstemci IP adresi
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Tarayıcı/İstemci bilgisi
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Ek veri/özellikler (JSON formatında)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// Log zaman damgası (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}