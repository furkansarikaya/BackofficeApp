using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Auditing;

namespace Backoffice.Application.Services.Interfaces;

public interface IActivityLogService
{
    /// <summary>
    /// Yeni bir aktivite logu ekler
    /// </summary>
    Task LogActivityAsync(CreateActivityLogDto dto);
    
    /// <summary>
    /// Belirtilen filtrelere göre aktivite loglarını getirir
    /// </summary>
    Task<PaginatedList<ActivityLogDto>> GetActivityLogsAsync(
        ActivityLogFilterDto filters, 
        int pageIndex, 
        int pageSize);
    
    /// <summary>
    /// ID'ye göre tek bir aktivite logu getirir
    /// </summary>
    Task<ActivityLogDto?> GetActivityLogByIdAsync(long id);
    
    /// <summary>
    /// Belirli bir varlığa (entity) ait aktivite loglarını getirir
    /// </summary>
    Task<List<ActivityLogDto>> GetEntityActivityLogsAsync(string entityType, string entityId);
    
    /// <summary>
    /// Belirli bir kullanıcıya ait aktivite loglarını getirir
    /// </summary>
    Task<PaginatedList<ActivityLogDto>> GetUserActivityLogsAsync(
        string userId, 
        int pageIndex, 
        int pageSize);
    
    /// <summary>
    /// Kategori türüne göre mevcut aktivite türlerini getirir
    /// </summary>
    List<string> GetActivityTypesByCategory(string category);
    
    /// <summary>
    /// Tüm aktivite kategorilerini getirir
    /// </summary>
    List<string> GetAllCategories();
}