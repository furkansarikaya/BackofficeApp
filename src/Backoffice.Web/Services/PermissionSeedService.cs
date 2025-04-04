using System.Reflection;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Enums;
using Backoffice.Infrastructure.Data;
using Backoffice.Infrastructure.Identity;
using Backoffice.Web.Attributes;
using Backoffice.Web.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Backoffice.Web.Services;

/// <summary>
/// Uygulama başlatıldığında izinleri otomatik keşfeder ve veritabanına kaydeder
/// </summary>
public class PermissionSeedService(
    ApplicationDbContext dbContext,
    IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
   IDbLoggerService logger)
{
    /// <summary>
    /// Uygulama başladığında tüm izinleri otomatik keşfeder ve veritabanına kaydeder
    /// </summary>
    public async Task SeedPermissionsAsync()
    {
        try
        {
            await logger.LogInformationAsync("İzinler veritabanına kaydediliyor...", "DatabaseSeed");
            
            var permissions = new List<Permission>();
            var descriptors = actionDescriptorCollectionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>();

            foreach (var descriptor in descriptors)
            {
                // Controller adını al
                var controllerName = descriptor.ControllerName;
                
                // Action adını al
                var actionName = descriptor.ActionName;
                
                // Controller veya Action üzerinde Permission attribute'u var mı kontrol et
                var permissionAttribute = descriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>() ??
                    descriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>();
                    
                if (permissionAttribute != null)
                {
                    var permission = new Permission
                    {
                        Controller = controllerName,
                        Action = actionName,
                        Code = permissionAttribute.PermissionCode,
                        Description = $"{controllerName} - {permissionAttribute.Type}"
                    };
                    
                    permissions.Add(permission);
                }
            }
            
            // Eksik controller'lar için varsayılan izinleri oluştur
            await CreateDefaultPermissionsForControllers(permissions);
            var existingPermissions = await dbContext.Permissions.ToListAsync();
            // Veritabanında eksik izinleri ekle
            foreach (var permission in permissions)
            {
                var existingPermission = existingPermissions
                    .FirstOrDefault(p => p.Code == permission.Code);
                    
                if (existingPermission == null)
                {
                    await logger.LogInformationAsync($"Yeni izin ekleniyor: {permission.Code}", "DatabaseSeed");
                    existingPermissions.Add(permission);
                    await dbContext.Permissions.AddAsync(permission);
                }
                else if(existingPermission.Id > 0)
                {
                    // Var olan izin bilgilerini güncelle
                    existingPermission.Controller = permission.Controller;
                    existingPermission.Action = permission.Action;
                    existingPermission.Description = permission.Description;
                    dbContext.Permissions.Update(existingPermission);
                }
            }

            await dbContext.SaveChangesAsync();
            await logger.LogInformationAsync("İzinler başarıyla kaydedildi","DatabaseSeed");
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync("İzinler kaydedilirken hata oluştu", "DatabaseSeed", ex);
        }
    }
    
    /// <summary>
    /// Temel controller'lar için varsayılan izinleri oluşturur
    /// </summary>
    private async Task CreateDefaultPermissionsForControllers(List<Permission> permissions)
    {
        var controllerNames = new[] { "User", "Role", "Menu", "IpFilter", "ActivityLog", "DbLog", "Setting", "ScheduledTask" };
        var permissionTypes = Enum.GetValues<PermissionType>().Cast<PermissionType>();
        
        foreach (var controller in controllerNames)
        {
            foreach (var type in permissionTypes)
            {
                var code = PermissionHelper.GeneratePermissionCode(controller, type);
                
                // Bu izin zaten var mı kontrol et
                if (!permissions.Any(p => p.Code == code))
                {
                    permissions.Add(new Permission
                    {
                        Controller = controller,
                        Action = type.ToString(),
                        Code = code,
                        Description = $"{controller} - {type}"
                    });
                }
            }
        }
    }
}