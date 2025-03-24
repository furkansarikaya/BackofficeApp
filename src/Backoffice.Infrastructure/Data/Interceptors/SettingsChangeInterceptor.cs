using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Entities.Settings;
using Backoffice.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Infrastructure.Data.Interceptors;

public class SettingsChangeInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private bool _settingsChanged = false;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, 
            InterceptionResult<int> result)
        {
            DetectSettingsChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            DetectSettingsChanges(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        public override int SavedChanges(
            SaveChangesCompletedEventData eventData, 
            int result)
        {
            if (!_settingsChanged) 
                return base.SavedChanges(eventData, result);
            
            Task.Run(async () => await RefreshSettingsAsync()).ConfigureAwait(false);
            _settingsChanged = false;

            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, 
            int result,
            CancellationToken cancellationToken = default)
        {
            if (!_settingsChanged) 
                return await base.SavedChangesAsync(eventData, result, cancellationToken);
            
            await RefreshSettingsAsync();
            _settingsChanged = false;

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void DetectSettingsChanges(DbContext context)
        {
            if (context == null) return;

            // Settings değişikliklerini tespit et
            var settingsEntries = context.ChangeTracker.Entries<Setting>()
                .Where(e => e.State == EntityState.Added 
                         || e.State == EntityState.Modified 
                         || e.State == EntityState.Deleted)
                .ToList();

            if (settingsEntries.Count != 0)
            {
                _settingsChanged = true;
            }
        }
        
        private async Task RefreshSettingsAsync()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                
                // Önbelleği temizle
                await settingsService.RefreshCacheAsync();
                
                // ISettings uygulamalarını yenile
                await RefreshAllSettingsInstancesAsync(scope.ServiceProvider);
            }
            catch
            {
                // Loglama yapılabilir
            }
        }
        
        private static async Task RefreshAllSettingsInstancesAsync(IServiceProvider serviceProvider)
        {
            var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            
            // Tüm ISettings uygulamalarını bul
            var settingsTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ISettings).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var settingsType in settingsTypes)
            {
                try
                {
                    // Servis olarak kayıtlı instance'ı al 
                    var settings = serviceProvider.GetService(settingsType);
                    if (settings == null) continue;
                    // Reflection ile BindSettingsAsync metodunu çağır
                    var bindMethod = typeof(ISettingsService).GetMethod(nameof(ISettingsService.BindSettingsAsync));
                    var genericBindMethod = bindMethod.MakeGenericMethod(settingsType);
                    await (Task)genericBindMethod.Invoke(settingsService, new[] { settings, null });
                }
                catch
                {
                    // Loglama yapılabilir
                }
            }
        }
    }