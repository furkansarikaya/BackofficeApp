using Backoffice.Domain.Enums;

namespace Backoffice.Application.DTOs.Logging;

/// <summary>
/// Log girişi DTO
/// </summary>
public class LogEntryDto
{
    /// <summary>
    /// Log ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Log seviyesi
    /// </summary>
    public LogLevel Level { get; set; }
    
    /// <summary>
    /// Log mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Log kategorisi
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Exception mesajı (varsa)
    /// </summary>
    public string? Exception { get; set; }
    
    /// <summary>
    /// Exception stack trace (varsa)
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// Kullanıcı ID (varsa)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Kullanıcı adı (varsa)
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// İstek URL (varsa)
    /// </summary>
    public string? RequestPath { get; set; }
    
    /// <summary>
    /// İstek metodu (varsa)
    /// </summary>
    public string? RequestMethod { get; set; }
    
    /// <summary>
    /// İstemci IP adresi
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// İstemci bilgisi
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Ek veri (JSON)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// Log zamanı (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
}