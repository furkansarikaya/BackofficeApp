using Backoffice.Application.Common.Models;
using Backoffice.Domain.Entities.Logging;
using Backoffice.Domain.Enums;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Veritabanına log kaydı yapan servis arayüzü
/// </summary>
public interface IDbLoggerService
{
    /// <summary>
    /// Log kaydı ekler
    /// </summary>
    Task LogAsync(LogLevel level, string message, string category, Exception? exception = null, object? additionalData = null);
    
    /// <summary>
    /// Information seviyesinde log kaydı ekler
    /// </summary>
    Task LogInformationAsync(string message, string category, object? additionalData = null);
    
    /// <summary>
    /// Warning seviyesinde log kaydı ekler
    /// </summary>
    Task LogWarningAsync(string message, string category, object? additionalData = null);
    
    /// <summary>
    /// Error seviyesinde log kaydı ekler
    /// </summary>
    Task LogErrorAsync(string message, string category, Exception? exception = null, object? additionalData = null);
    
    /// <summary>
    /// Critical seviyesinde log kaydı ekler
    /// </summary>
    Task LogCriticalAsync(string message, string category, Exception? exception = null, object? additionalData = null);
    
    /// <summary>
    /// Belirli filtrelere göre log kayıtlarını getirir
    /// </summary>
    Task<PaginatedList<LogEntry>> GetLogsAsync(
        int pageIndex, 
        int pageSize, 
        LogLevel? level = null, 
        string? category = null, 
        string? searchTerm = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        string? userId = null);
    
    /// <summary>
    /// ID'ye göre log kaydı getirir
    /// </summary>
    Task<LogEntry?> GetLogByIdAsync(long id);
    
    /// <summary>
    /// Belirli bir kategori için istatistikleri getirir
    /// </summary>
    Task<Dictionary<LogLevel, int>> GetLogStatisticsByCategoryAsync(string category, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Tüm kategorileri getirir
    /// </summary>
    Task<List<string>> GetAllCategoriesAsync();
}