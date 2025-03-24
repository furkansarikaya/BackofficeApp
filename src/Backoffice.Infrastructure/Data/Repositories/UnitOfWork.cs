using System.Reflection;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Entities.Common;
using Backoffice.Domain.Settings;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Infrastructure.Data.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext,IServiceProvider serviceProvider) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public IGenericRepository<T, TKey> Repository<T, TKey>() where T : BaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        var type = typeof(T);

        if (_repositories.TryGetValue(type, out var value)) return (IGenericRepository<T, TKey>)value;
        var repository = new GenericRepository<T, TKey>(dbContext);
        value = repository;
        _repositories.Add(type, value);

        return (IGenericRepository<T, TKey>)value;
    }

    public async Task<int> SaveChangesAsync()
    {
        var result = await dbContext.SaveChangesAsync();
        return result;
    }

    public async Task<bool> SaveChangesAndClearCacheAsync(params string[] cacheKeys)
    {
        var result = await dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            await _transaction?.CommitAsync()!;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction?.RollbackAsync()!;
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}