using System.Linq.Expressions;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;
using Backoffice.Domain.Entities.Common;
using Backoffice.Domain.Specifications;
using Backoffice.Infrastructure.Data.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Backoffice.Infrastructure.Data.Repositories;

public class GenericRepository<T,TKey> : IGenericRepository<T,TKey> 
    where T : BaseEntity<TKey> where TKey : IEquatable<TKey>
{
    protected readonly ApplicationDbContext DbContext;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        _dbSet = DbContext.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        DbContext.Entry(entity).State = EntityState.Modified;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    public virtual async Task<IReadOnlyList<T>> GetWithIncludeStringAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeString = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        
        if (disableTracking) 
            query = query.AsNoTracking();
            
        query = query.ApplySpecification(predicate)
                     .ApplyInclude(includeString)
                     .ApplyOrder(orderBy);
                     
        return await query.ToListAsync();
    }

    public virtual async Task<IReadOnlyList<T>> GetWithIncludesAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        
        if (disableTracking) 
            query = query.AsNoTracking();
            
        query = query.ApplySpecification(predicate)
                     .ApplyInclude(includes)
                     .ApplyOrder(orderBy);
                     
        return await query.ToListAsync();
    }

    public virtual async Task<IReadOnlyList<T>> GetAsync(BaseSpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public virtual async Task<PaginatedList<T>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        
        if (disableTracking) 
            query = query.AsNoTracking();
            
        query = query.ApplySpecification(predicate)
                     .ApplyInclude(includes)
                     .ApplyOrder(orderBy);
                     
        return await query.ToPaginatedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<PaginatedList<T>> GetPagedWithFilterAsync(
        FilterModel filter,
        int pageIndex,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        
        if (disableTracking) 
            query = query.AsNoTracking();
            
        query = query.ApplyFilter(filter)
                     .ApplyInclude(includes)
                     .ApplyOrder(orderBy);
                     
        return await query.ToPaginatedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = _dbSet;
        
        if (predicate != null)
            query = query.Where(predicate);
            
        return await query.CountAsync();
    }

    public virtual IQueryable<T> GetQueryable(bool disableTracking = true)
    {
        if (disableTracking)
            return _dbSet.AsNoTracking();
            
        return _dbSet;
    }
    
    private IQueryable<T> ApplySpecification(BaseSpecification<T> spec)
    {
        IQueryable<T> query = _dbSet;
        
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);
            
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
        
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
            
        if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);
            
        if (spec.GroupBy != null)
            query = query.GroupBy(spec.GroupBy).SelectMany(x => x);
            
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);
            
        return query;
    }
}