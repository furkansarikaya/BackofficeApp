using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(
    IServiceProvider serviceProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var score = serviceProvider.CreateScope();
        var currentUserService = score.ServiceProvider.GetRequiredService<ICurrentUserService>();
        var dateTime = score.ServiceProvider.GetRequiredService<IDateTimeService>();
        
        var userId = currentUserService.UserId;
        var now = dateTime.Now;
        
        //Create işlemi
        foreach (var entry in context.ChangeTracker.Entries<ICreationAuditableEntity>())
        {
            if (entry.State != EntityState.Added) continue;
            // Oluşturma işlemi
            entry.Entity.CreatedBy = userId;
            entry.Entity.CreatedAt = now;
        }
        
        // Update işlemi
        foreach (var entry in context.ChangeTracker.Entries<IModificationAuditableEntity>())
        {
            if (entry.State != EntityState.Modified) continue;
            // Güncelleme işlemi
            entry.Entity.LastModifiedBy = userId;
            entry.Entity.LastModifiedAt = now;
        }

        // Soft Delete işlemi
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State != EntityState.Deleted) continue;
            // Silme işlemini iptal et ve entity'yi güncelleme olarak işaretle
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedBy = userId;
            entry.Entity.DeletedAt = now;
        }
    }
}