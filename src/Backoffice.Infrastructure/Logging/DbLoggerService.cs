using System.Linq.Expressions;
using System.Text.Json;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;
using Backoffice.Domain.Entities.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LogLevel = Backoffice.Domain.Enums.LogLevel;

namespace Backoffice.Infrastructure.Logging;

/// <summary>
/// Veritabanına log kaydı yapan servis implementasyonu
/// </summary>
public class DbLoggerService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<DbLoggerService> logger)
    : IDbLoggerService
{
    /// <inheritdoc />
    public async Task LogAsync(LogLevel level, string message, string category, Exception? exception = null, object? additionalData = null)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            
            var logEntry = new LogEntry
            {
                Level = level,
                Message = message,
                Category = category,
                Exception = exception?.Message,
                StackTrace = exception?.StackTrace,
                UserId = currentUserService.IsAuthenticated ? currentUserService.UserId : null,
                UserName = currentUserService.IsAuthenticated ? currentUserService.UserName : null,
                RequestPath = httpContext?.Request.Path.Value,
                RequestMethod = httpContext?.Request.Method,
                IpAddress = currentUserService.GetClientIp,
                UserAgent = currentUserService.GetUserAgent,
                AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                Timestamp = DateTime.UtcNow
            };

            var repository = unitOfWork.Repository<LogEntry, long>();
            await repository.AddAsync(logEntry);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Loglama servisinde hata olursa Microsoft.Extensions.Logging kullan
            logger.LogError(ex, "Veritabanı log kaydı oluşturulurken hata oluştu: {Message}", ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task LogInformationAsync(string message, string category, object? additionalData = null)
    {
        await LogAsync(LogLevel.Information, message, category, null, additionalData);
    }

    /// <inheritdoc />
    public async Task LogWarningAsync(string message, string category, object? additionalData = null)
    {
        await LogAsync(LogLevel.Warning, message, category, null, additionalData);
    }

    /// <inheritdoc />
    public async Task LogErrorAsync(string message, string category, Exception? exception = null, object? additionalData = null)
    {
        await LogAsync(LogLevel.Error, message, category, exception, additionalData);
    }

    /// <inheritdoc />
    public async Task LogCriticalAsync(string message, string category, Exception? exception = null, object? additionalData = null)
    {
        await LogAsync(LogLevel.Critical, message, category, exception, additionalData);
    }

    /// <inheritdoc />
    public async Task<PaginatedList<LogEntry>> GetLogsAsync(
        int pageIndex, 
        int pageSize, 
        LogLevel? level = null, 
        string? category = null, 
        string? searchTerm = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        string? userId = null)
    {
        var repository = unitOfWork.Repository<LogEntry, long>();
        
        // Filtreleri oluştur
        Expression<Func<LogEntry, bool>>? predicate = null;
        
        // Log seviyesi filtresi
        if (level.HasValue)
        {
            Expression<Func<LogEntry, bool>> levelFilter = x => x.Level == level.Value;
            predicate = CombineExpressions(predicate, levelFilter);
        }
        
        // Kategori filtresi
        if (!string.IsNullOrEmpty(category))
        {
            Expression<Func<LogEntry, bool>> categoryFilter = x => x.Category == category;
            predicate = CombineExpressions(predicate, categoryFilter);
        }
        
        // Kullanıcı ID filtresi
        if (!string.IsNullOrEmpty(userId))
        {
            Expression<Func<LogEntry, bool>> userIdFilter = x => x.UserId == userId;
            predicate = CombineExpressions(predicate, userIdFilter);
        }
        
        // Tarih aralığı filtresi
        if (fromDate.HasValue)
        {
            Expression<Func<LogEntry, bool>> fromDateFilter = x => x.Timestamp >= fromDate.Value;
            predicate = CombineExpressions(predicate, fromDateFilter);
        }
        
        if (toDate.HasValue)
        {
            var toDateEnd = toDate.Value.AddDays(1).AddMilliseconds(-1);
            Expression<Func<LogEntry, bool>> toDateFilter = x => x.Timestamp <= toDateEnd;
            predicate = CombineExpressions(predicate, toDateFilter);
        }
        
        // Arama terimi filtresi
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Expression<Func<LogEntry, bool>> searchFilter = x => 
                x.Message.Contains(searchTerm) || 
                x.Category.Contains(searchTerm) || 
                (x.Exception != null && x.Exception.Contains(searchTerm)) ||
                (x.UserName != null && x.UserName.Contains(searchTerm));
                
            predicate = CombineExpressions(predicate, searchFilter);
        }
        
        // Sıralama ve sayfalama
        var result = await repository.GetPagedAsync(
            pageIndex, 
            pageSize, 
            predicate, 
            q => q.OrderByDescending(x => x.Timestamp));
        
        return new PaginatedList<LogEntry>(
            result.Items.ToList(),
            result.TotalCount,
            result.PageIndex,
            result.PageSize);
    }

    /// <inheritdoc />
    public async Task<LogEntry?> GetLogByIdAsync(long id)
    {
        var repository = unitOfWork.Repository<LogEntry, long>();
        return await repository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<Dictionary<LogLevel, int>> GetLogStatisticsByCategoryAsync(string category, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var repository = unitOfWork.Repository<LogEntry, long>();
        var query = repository.GetQueryable();
        
        // Kategori filtresi
        query = query.Where(x => x.Category == category);
        
        // Tarih aralığı filtresi
        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromDate.Value);
        }
        
        if (toDate.HasValue)
        {
            var toDateEnd = toDate.Value.AddDays(1).AddMilliseconds(-1);
            query = query.Where(x => x.Timestamp <= toDateEnd);
        }
        
        // Seviyeye göre grupla ve say
        var statistics = await query
            .GroupBy(x => x.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Level, x => x.Count);
        
        // Tüm log seviyelerini içeren bir dictionary döndür
        var result = new Dictionary<LogLevel, int>();
        
        foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
        {
            if (level != LogLevel.None) // None seviyesi hariç
            {
                result[level] = statistics.TryGetValue(level, out var count) ? count : 0;
            }
        }
        
        return result;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        var repository = unitOfWork.Repository<LogEntry, long>();
        
        return await repository.GetQueryable()
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }
    
    #region Helper Methods
    
    /// <summary>
    /// İki expression'ı AND ile birleştirir
    /// </summary>
    private Expression<Func<LogEntry, bool>>? CombineExpressions(
        Expression<Func<LogEntry, bool>>? expr1,
        Expression<Func<LogEntry, bool>> expr2)
    {
        if (expr1 == null)
        {
            return expr2;
        }
        
        var parameter = Expression.Parameter(typeof(LogEntry), "x");
        
        var visitor1 = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
        var left = visitor1.Visit(expr1.Body);
        
        var visitor2 = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
        var right = visitor2.Visit(expr2.Body);
        
        var combined = Expression.AndAlso(left, right);
        
        return Expression.Lambda<Func<LogEntry, bool>>(combined, parameter);
    }
    
    private class ReplaceParameterVisitor(Expression oldParameter, ParameterExpression newParameter)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }
    
    #endregion
}