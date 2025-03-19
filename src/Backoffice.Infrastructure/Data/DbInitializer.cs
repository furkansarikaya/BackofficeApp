using System.Security.Claims;
using Backoffice.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backoffice.Infrastructure.Data;

/// <summary>
/// Veritabanı başlangıç verilerini yüklemek için kullanılan yardımcı sınıf
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Veritabanını başlatır ve gerekli seed verilerini yükler.
    /// Bu metot uygulama ilk çalıştığında veya veritabanı boş olduğunda kullanılmalıdır.
    /// </summary>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Veritabanı başlatılıyor...");

            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            // Veritabanı migrasyon kontrolü
            // Veritabanı migrasyon kontrolü ve uygulama
            if (context.Database.IsRelational())
            {
                try
                {
                    logger.LogInformation("Veritabanı migrasyonları uygulanıyor...");
        
                    // Bekleyen migrasyonları kontrol et
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    var pendingCount = pendingMigrations.Count();
        
                    if (pendingCount > 0)
                    {
                        logger.LogInformation("{0} adet bekleyen migration bulundu:", pendingCount);
                        foreach (var migration in pendingMigrations)
                        {
                            logger.LogInformation("  - {0}", migration);
                        }
            
                        // Migrasyonları uygula
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Tüm migrasyonlar başarıyla uygulandı.");
                    }
                    else
                    {
                        logger.LogInformation("Bekleyen migration bulunamadı. Veritabanı güncel.");
                    }
        
                    // Uygulanan migrasyonları göster
                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                    logger.LogInformation("{0} adet migration uygulanmış durumda:", appliedMigrations.Count());
                    foreach (var migration in appliedMigrations)
                    {
                        logger.LogInformation("  - {0}", migration);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Migration uygulanırken bir hata oluştu: {0}", ex.Message);
                    throw;
                }
            }

            // Rolleri oluştur
            await SeedRolesAsync(roleManager, logger);

            // Kullanıcıları oluştur
            await SeedUsersAsync(userManager, roleManager, logger);

            logger.LogInformation("Veritabanı başarıyla başlatıldı.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
            throw;
        }
    }

    /// <summary>
    /// Temel rolleri oluşturur ve izinleri ayarlar
    /// </summary>
    private static async Task SeedRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        logger.LogInformation("Roller oluşturuluyor...");

        // Temel rolleri tanımla
        var roles = new List<(string Name, string Description)>
        {
            ("Administrator", "Sistem yöneticisi, tüm izinlere sahiptir."),
            ("Editor", "İçerik düzenleyicisi, düzenleme yetkisine sahiptir."),
            ("Viewer", "İzleyici, sadece görüntüleme yapabilir.")
        };

        foreach (var (name, description) in roles)
        {
            // Rol var mı kontrol et
            if (!await roleManager.RoleExistsAsync(name))
            {
                var role = new ApplicationRole
                {
                    Name = name,
                    Description = description
                };

                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    logger.LogInformation("Rol oluşturuldu: {Name}", name);

                    // Rollere izinleri ekle (Administrator hariç, o tüm izinlere sahip olacak)
                    if (name != "Administrator")
                    {
                        await SetRolePermissionsAsync(roleManager, name, logger);
                    }
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Rol oluşturulamadı {Name}: {Errors}", name, errors);
                }
            }
            else
            {
                logger.LogInformation("Rol zaten mevcut: {Name}", name);
            }
        }
    }

    /// <summary>
    /// Rol izinlerini ayarlar
    /// </summary>
    private static async Task SetRolePermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        string roleName,
        ILogger logger)
    {
        var role = await roleManager.FindByNameAsync(roleName);

        if (role == null)
            return;

        // Her rol için izinleri tanımla
        var permissions = new List<string>();

        switch (roleName)
        {
            case "Editor":
                // Düzenleme izinleri (silme hariç)
                permissions.AddRange(new[]
                {
                    "Categories.List",
                    "Categories.View",
                    "Categories.Create",
                    "Categories.Update",
                    "Products.List",
                    "Products.View",
                    "Products.Create",
                    "Products.Update"
                });
                break;

            case "Viewer":
                // Sadece görüntüleme izinleri
                permissions.AddRange(new[]
                {
                    "Categories.List",
                    "Categories.View",
                    "Products.List",
                    "Products.View"
                });
                break;
        }

        // İzinleri role ekle
        foreach (var permission in permissions)
        {
            var hasClaim = (await roleManager.GetClaimsAsync(role))
                .Any(c => c.Type == "Permission" && c.Value == permission);

            if (!hasClaim)
            {
                var claim = new Claim("Permission", permission);
                var result = await roleManager.AddClaimAsync(role, claim);

                if (result.Succeeded)
                {
                    logger.LogInformation("İzin eklendi: {Role} -> {Permission}", roleName, permission);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("İzin eklenemedi {Role} -> {Permission}: {Errors}",
                        roleName, permission, errors);
                }
            }
        }
    }

    /// <summary>
    /// Test kullanıcılarını oluşturur ve rollere atar
    /// </summary>
    private static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        logger.LogInformation("Kullanıcılar oluşturuluyor...");

        // Test kullanıcılarını oluştur
        var users = new List<(string Email, string Password, string FirstName, string LastName, string[] Roles)>
        {
            // Admin kullanıcısı
            ("admin@example.com", "Admin123!", "Admin", "User", new[] { "Administrator" }),

            // Diğer rol kullanıcıları
            ("editor@example.com", "Editor123!", "Editor", "User", new[] { "Editor" }),
            ("viewer@example.com", "Viewer123!", "Viewer", "User", new[] { "Viewer" })
        };

        foreach (var (email, password, firstName, lastName, roles) in users)
        {
            // Kullanıcı var mı kontrol et
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                // Yeni kullanıcı oluştur
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    logger.LogInformation("Kullanıcı oluşturuldu: {Email}", email);

                    // Rolleri ekle
                    foreach (var role in roles)
                    {
                        if (await roleManager.RoleExistsAsync(role))
                        {
                            await userManager.AddToRoleAsync(user, role);
                            logger.LogInformation("Kullanıcıya rol eklendi: {Email} -> {Role}", email, role);
                        }
                    }
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Kullanıcı oluşturulamadı {Email}: {Errors}", email, errors);
                }
            }
            else
            {
                logger.LogInformation("Kullanıcı zaten mevcut: {Email}", email);
            }
        }
    }
}