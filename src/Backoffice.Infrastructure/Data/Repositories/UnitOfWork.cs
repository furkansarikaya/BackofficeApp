using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backoffice.Infrastructure.Data.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
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
        return await dbContext.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAndClearCacheAsync(params string[] cacheKeys)
    {
        // Cache temizleme özelliği eklenecek
        // Şimdilik sadece kaydetme işlemini yapıyor
        await dbContext.SaveChangesAsync();
        return true;
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