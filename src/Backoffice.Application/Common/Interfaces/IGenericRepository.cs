using System.Linq.Expressions;
using Backoffice.Application.Common.Models;
using Backoffice.Domain.Entities.Common;
using Backoffice.Domain.Specifications;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Tüm varlıklar için generic repository arayüzü
/// </summary>
public interface IGenericRepository<T,TKey> where T : BaseEntity<TKey> where TKey : IEquatable<TKey>
{
    // Temel CRUD Operasyonları
    Task<T?> GetByIdAsync(TKey id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();

    // Gelişmiş Sorgulama
    Task<IReadOnlyList<T>> GetWithIncludeStringAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeString = null,
        bool disableTracking = true);

    Task<IReadOnlyList<T>> GetWithIncludesAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true);
    
    // Specification pattern kullanarak sorgulama
    Task<IReadOnlyList<T>> GetAsync(BaseSpecification<T> spec);
    
    // Sayfalama için
    Task<PaginatedList<T>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true);
    
    // Dinamik Filtreleme
    Task<PaginatedList<T>> GetPagedWithFilterAsync(
        FilterModel filter,
        int pageIndex,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true);
    
    // Sayım
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    // Raw IQueryable (ileri düzey LINQ sorguları için)
    IQueryable<T> GetQueryable(bool disableTracking = true);
}