namespace Backoffice.Domain.Enums;

/// <summary>
/// Log kayıtları için seviye tanımları
/// </summary>
public enum LogLevel : byte
{
    /// <summary>
    /// Detaylı hata ayıklama bilgileri
    /// </summary>
    Trace = 0,
    
    /// <summary>
    /// Hata ayıklama bilgileri
    /// </summary>
    Debug = 1,
    
    /// <summary>
    /// Genel bilgi mesajları
    /// </summary>
    Information = 2,
    
    /// <summary>
    /// Uyarı mesajları
    /// </summary>
    Warning = 3,
    
    /// <summary>
    /// Hata mesajları
    /// </summary>
    Error = 4,
    
    /// <summary>
    /// Kritik hatalar
    /// </summary>
    Critical = 5,
    
    /// <summary>
    /// Hiçbir log kaydedilmez
    /// </summary>
    None = 6
}