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
        await RefreshAllSettingsInstancesAsync();
        return result;
    }

    public async Task<bool> SaveChangesAndClearCacheAsync(params string[] cacheKeys)
    {
        var result = await dbContext.SaveChangesAsync();
        await RefreshAllSettingsInstancesAsync();
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
    
    private async Task RefreshAllSettingsInstancesAsync()
    {
        try
        {
            // Tüm register edilmiş ISettings uygulamalarını yenile
            using var scope = serviceProvider.CreateScope();
            var settingsService = scope.ServiceProvider.GetService<ISettingsService>();
            // Çalışan uygulamadaki tüm assembly'leri al
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            // Henüz yüklenmemiş assembly'leri yükle
            var referencedAssemblies = Assembly.GetEntryAssembly()?
                .GetReferencedAssemblies()
                .Where(a => assemblies.All(loaded => loaded.GetName().Name != a.Name))
                .Select(Assembly.Load)
                .ToList();

            if (referencedAssemblies != null)
            {
                assemblies.AddRange(referencedAssemblies);
            }  
            
            var settingsTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ISettings).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .ToList();

            foreach (var settingsType in settingsTypes)
            {
                try
                {
                    // Generik olmayan tip için object olarak al, sonra doğru tipe cast edeceğiz
                    var settings = scope.ServiceProvider.GetService(settingsType);
                    if (settings != null)
                    {
                        // Reflection ile BindSettingsAsync metodunu çağır
                        var bindMethod = typeof(ISettingsService).GetMethod(nameof(ISettingsService.BindSettingsAsync));
                        var genericBindMethod = bindMethod.MakeGenericMethod(settingsType);
                        await (Task)genericBindMethod.Invoke(settingsService, new[] { settings, null });
                    }
                }
                catch
                {
                    // Loglama eklenebilir
                }
            }
        }
        catch
        {
            // Global hata durumunda loglama eklenebilir
        }
    }
}