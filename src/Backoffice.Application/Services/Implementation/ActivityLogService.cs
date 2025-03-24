using System.Linq.Expressions;
using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Auditing;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Constants;
using Backoffice.Domain.Entities.Auditing;

namespace Backoffice.Application.Services.Implementation;

public class ActivityLogService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IDbLoggerService logger)
    : IActivityLogService
{
    public async Task LogActivityAsync(CreateActivityLogDto dto)
    {
        try
        {
            var repository = unitOfWork.Repository<ActivityLog, long>();
            
            var activityLog = new ActivityLog
            {
                UserId = currentUserService.UserId,
                UserName = currentUserService.UserName,
                Category = dto.Category,
                ActivityType = dto.ActivityType,
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                Details = dto.Details,
                IpAddress = currentUserService.GetClientIp,
                UserAgent = currentUserService.GetUserAgent,
                Timestamp = DateTime.UtcNow
            };
            
            await repository.AddAsync(activityLog);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Aktivite logu kaydedilirken hata oluştu: {ex.Message}", "ActivityLogService", ex);
            // Log işlemi başarısız olsa bile uygulamanın çalışmasını engellememek için
            // exception fırlatmıyoruz, sadece hata logunu kaydediyoruz
        }
    }

    public async Task<PaginatedList<ActivityLogDto>> GetActivityLogsAsync(
        ActivityLogFilterDto filters,
        int pageIndex,
        int pageSize)
    {
        var repository = unitOfWork.Repository<ActivityLog, long>();
        
        // Filtreleri oluştur
        var predicate = BuildFilterPredicate(filters);
        
        // Sıralama tanımı
        var orderBy = ApplySorting(q => q.OrderByDescending(x => x.Timestamp));
        
        // Sayfalı sonuçları getir
        var result = await repository.GetPagedAsync(
            pageIndex, 
            pageSize, 
            predicate, 
            orderBy);
        
        // Sonuçları DTO'lara dönüştür
        var dtos = mapper.Map<List<ActivityLogDto>>(result.Items);
        
        return new PaginatedList<ActivityLogDto>(
            dtos,
            result.TotalCount,
            pageIndex,
            pageSize);
    }

    public async Task<ActivityLogDto?> GetActivityLogByIdAsync(long id)
    {
        var repository = unitOfWork.Repository<ActivityLog, long>();
        var activityLog = await repository.GetByIdAsync(id);
        
        return activityLog == null ? null : mapper.Map<ActivityLogDto>(activityLog);
    }

    public async Task<List<ActivityLogDto>> GetEntityActivityLogsAsync(string entityType, string entityId)
    {
        var repository = unitOfWork.Repository<ActivityLog, long>();
        
        var logs = await repository.GetWithIncludeStringAsync(
            predicate: x => x.EntityType == entityType && x.EntityId == entityId,
            orderBy: q => q.OrderByDescending(x => x.Timestamp));
        
        return mapper.Map<List<ActivityLogDto>>(logs);
    }

    public async Task<PaginatedList<ActivityLogDto>> GetUserActivityLogsAsync(
        string userId,
        int pageIndex,
        int pageSize)
    {
        var repository = unitOfWork.Repository<ActivityLog, long>();
        
        var result = await repository.GetPagedAsync(
            pageIndex, 
            pageSize, 
            predicate: x => x.UserId == userId, 
            orderBy: q => q.OrderByDescending(x => x.Timestamp));
        
        var dtos = mapper.Map<List<ActivityLogDto>>(result.Items);
        
        return new PaginatedList<ActivityLogDto>(
            dtos,
            result.TotalCount,
            pageIndex,
            pageSize);
    }

    public List<string> GetActivityTypesByCategory(string category)
    {
        // Bu metod, kategoriye göre aktivite tiplerini döndürür
        // Bu örnekte sabit değerler kullanıyoruz, ancak bu veriyi veritabanından da çekebilirsiniz

        return category switch
        {
            ActivityCategories.Authentication => [ActivityTypes.Login, ActivityTypes.Logout, ActivityTypes.FailedLogin, ActivityTypes.PasswordChanged, ActivityTypes.PasswordReset],
            ActivityCategories.UserManagement => [ActivityTypes.Create, ActivityTypes.Update, ActivityTypes.Delete, ActivityTypes.ChangeStatus, ActivityTypes.ChangeRole],
            ActivityCategories.RoleManagement => [ActivityTypes.Create, ActivityTypes.Update, ActivityTypes.Delete, ActivityTypes.GrantPermission, ActivityTypes.RevokePermission],
            ActivityCategories.MenuManagement or ActivityCategories.IpFilterManagement => [ActivityTypes.Create, ActivityTypes.Update, ActivityTypes.Delete, ActivityTypes.ChangeStatus],
            ActivityCategories.SystemConfiguration => [ActivityTypes.SystemStartup, ActivityTypes.SystemShutdown, ActivityTypes.ConfigChange, ActivityTypes.BackupCreate, ActivityTypes.BackupRestore],
            _ => [ActivityTypes.Create, ActivityTypes.Update, ActivityTypes.Delete, ActivityTypes.View]
        };
    }

    public List<string> GetAllCategories()
    {
        // Tüm kategorileri döndürür
        // Bu örnekte sabit değerler kullanıyoruz, ancak bu veriyi veritabanından da çekebilirsiniz
        
        return
        [
            ActivityCategories.Authentication,
            ActivityCategories.UserManagement,
            ActivityCategories.RoleManagement,
            ActivityCategories.MenuManagement,
            ActivityCategories.IpFilterManagement,
            ActivityCategories.SystemConfiguration,
            ActivityCategories.DataManagement,
            ActivityCategories.Security
        ];
    }

    #region Helper Methods
    
    private Expression<Func<ActivityLog, bool>>? BuildFilterPredicate(ActivityLogFilterDto filters)
    {
        // Linq expression builder kullanarak dinamik filtre oluşturma
        Expression<Func<ActivityLog, bool>>? predicate = null;
        
        // UserId filtresi
        if (!string.IsNullOrEmpty(filters.UserId))
        {
            Expression<Func<ActivityLog, bool>> userIdFilter = x => x.UserId == filters.UserId;
            predicate = predicate == null ? userIdFilter : CombinePredicates(predicate, userIdFilter);
        }
        
        // Category filtresi
        if (!string.IsNullOrEmpty(filters.Category))
        {
            Expression<Func<ActivityLog, bool>> categoryFilter = x => x.Category == filters.Category;
            predicate = predicate == null ? categoryFilter : CombinePredicates(predicate, categoryFilter);
        }
        
        // ActivityType filtresi
        if (!string.IsNullOrEmpty(filters.ActivityType))
        {
            Expression<Func<ActivityLog, bool>> activityTypeFilter = x => x.ActivityType == filters.ActivityType;
            predicate = predicate == null ? activityTypeFilter : CombinePredicates(predicate, activityTypeFilter);
        }
        
        // EntityType filtresi
        if (!string.IsNullOrEmpty(filters.EntityType))
        {
            Expression<Func<ActivityLog, bool>> entityTypeFilter = x => x.EntityType == filters.EntityType;
            predicate = predicate == null ? entityTypeFilter : CombinePredicates(predicate, entityTypeFilter);
        }
        
        // EntityId filtresi
        if (!string.IsNullOrEmpty(filters.EntityId))
        {
            Expression<Func<ActivityLog, bool>> entityIdFilter = x => x.EntityId == filters.EntityId;
            predicate = predicate == null ? entityIdFilter : CombinePredicates(predicate, entityIdFilter);
        }
        
        // Tarih aralığı filtreleri
        if (filters.FromDate.HasValue)
        {
            Expression<Func<ActivityLog, bool>> fromDateFilter = x => x.Timestamp >= filters.FromDate.Value;
            predicate = predicate == null ? fromDateFilter : CombinePredicates(predicate, fromDateFilter);
        }
        
        if (filters.ToDate.HasValue)
        {
            // ToDate'e 1 gün ekleyerek o günün sonuna kadar olan kayıtları dahil ediyoruz
            var toDateEnd = filters.ToDate.Value.AddDays(1).AddMilliseconds(-1);
            Expression<Func<ActivityLog, bool>> toDateFilter = x => x.Timestamp <= toDateEnd;
            predicate = predicate == null ? toDateFilter : CombinePredicates(predicate, toDateFilter);
        }
        
        // Arama terimi filtresi (UserName, Details veya EntityId içerisinde arama yapar)
        if (!string.IsNullOrEmpty(filters.SearchTerm))
        {
            Expression<Func<ActivityLog, bool>> searchFilter = x => 
                x.UserName.Contains(filters.SearchTerm) || 
                (x.Details != null && x.Details.Contains(filters.SearchTerm)) ||
                (x.EntityId != null && x.EntityId.Contains(filters.SearchTerm));
                
            predicate = predicate == null ? searchFilter : CombinePredicates(predicate, searchFilter);
        }
        
        return predicate;
    }
    
    private Expression<Func<ActivityLog, bool>> CombinePredicates(
        Expression<Func<ActivityLog, bool>> predicate1,
        Expression<Func<ActivityLog, bool>> predicate2)
    {
        // İki predicate'i AND ile birleştiren Expression Tree oluşturma
        var parameter = Expression.Parameter(typeof(ActivityLog), "x");
        
        var leftVisitor = new ReplaceExpressionVisitor(predicate1.Parameters[0], parameter);
        var left = leftVisitor.Visit(predicate1.Body);
        
        var rightVisitor = new ReplaceExpressionVisitor(predicate2.Parameters[0], parameter);
        var right = rightVisitor.Visit(predicate2.Body);
        
        return Expression.Lambda<Func<ActivityLog, bool>>(
            Expression.AndAlso(left, right), parameter);
    }
    
    private class ReplaceExpressionVisitor(Expression? oldValue, Expression? newValue) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            return node == oldValue ? newValue : base.Visit(node);
        }
    }
    
    private Func<IQueryable<ActivityLog>, IOrderedQueryable<ActivityLog>> ApplySorting(
        Func<IQueryable<ActivityLog>, IOrderedQueryable<ActivityLog>> defaultOrder)
    {
        // Burada farklı sıralama seçenekleri sunabilirsiniz
        // Şimdilik varsayılan sıralamayı kullanıyoruz
        return defaultOrder;
    }
    
    #endregion
}