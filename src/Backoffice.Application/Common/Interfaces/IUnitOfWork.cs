using Backoffice.Domain.Entities.Common;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Unit of Work patternini uygulayan arayüz
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T,TKey> Repository<T,TKey>() where T : BaseEntity<TKey> where TKey : IEquatable<TKey>;
    Task<int> SaveChangesAsync();
    Task<bool> SaveChangesAndClearCacheAsync(params string[] cacheKeys);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}