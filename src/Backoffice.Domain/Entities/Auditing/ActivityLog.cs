using Backoffice.Domain.Entities.Common;

namespace Backoffice.Domain.Entities.Auditing;

/// <summary>
/// Sistem içerisindeki önemli aktivitelerin kaydedildiği log tablosu
/// </summary>
public class ActivityLog : BaseEntity<long>
{
    /// <summary>
    /// İşlemi gerçekleştiren kullanıcı ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// İşlemi gerçekleştiren kullanıcı adı
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// İşlem kategorisi (User, Role, Menu, vb.)
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// İşlem tipi (Create, Update, Delete, Login, vb.)
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;
    
    /// <summary>
    /// İşleme konu olan varlık tipi (ApplicationUser, Role, MenuItem, vb.)
    /// </summary>
    public string? EntityType { get; set; }
    
    /// <summary>
    /// İşleme konu olan varlık ID'si
    /// </summary> 
    public string? EntityId { get; set; }
    
    /// <summary>
    /// İşlem detayı (JSON formatında değişiklikler veya bilgiler)
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// İstemci IP adresi
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Kullanıcı tarayıcı ve işletim sistemi bilgisi
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// İşlem tarihi ve zamanı (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}