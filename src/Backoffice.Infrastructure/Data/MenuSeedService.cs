using Backoffice.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backoffice.Infrastructure.Data;

/// <summary>
/// Seeds default menu items in the database
/// </summary>
public class MenuSeedService(
    ApplicationDbContext dbContext,
    ILogger<MenuSeedService> logger)
{
    /// <summary>
    /// Seeds the default menu items if no menu items exist
    /// </summary>
    public async Task SeedMenuItemsAsync()
    {
        try
        {
            // Only seed if no menu items exist
            if (await dbContext.MenuItems.AnyAsync())
            {
                logger.LogInformation("Menu items already exist, skipping seeding");
                return;
            }

            logger.LogInformation("Seeding menu items...");

            // Add default menu items
            var menuItems = new List<MenuItem>
            {
                // Dashboard
                new MenuItem
                {
                    Name = "Ana Sayfa",
                    Icon = "fas fa-home",
                    Controller = "Home",
                    Action = "Index",
                    DisplayOrder = 1,
                    IsActive = true
                },

                // User Management Section Header
                new MenuItem
                {
                    Name = "Kullanıcı Yönetimi",
                    Icon = "fas fa-user-cog",
                    DisplayOrder = 2,
                    IsSectionHeader = true,
                    IsActive = true,
                    Children = new List<MenuItem>
                    {
                        // Users
                        new MenuItem
                        {
                            Name = "Kullanıcılar",
                            Icon = "fas fa-users",
                            Controller = "User",
                            Action = "Index",
                            DisplayOrder = 3,
                            RequiredPermissionCode = "Users.List",
                            IsActive = true
                        },

                        // Roles
                        new MenuItem
                        {
                            Name = "Roller",
                            Icon = "fas fa-user-shield",
                            Controller = "Role",
                            Action = "Index",
                            DisplayOrder = 4,
                            RequiredPermissionCode = "Roles.List",
                            IsActive = true
                        }
                    }
                },
                // System Management Section Header
                new MenuItem
                {
                    Name = "Sistem Yönetimi",
                    Icon = "fas fa-cogs",
                    DisplayOrder = 900,
                    IsSectionHeader = true,
                    IsActive = true,
                    Children = new List<MenuItem>
                    {
                        // Menu Management
                        new MenuItem
                        {
                            Name = "Menü Yönetimi",
                            Icon = "fas fa-bars",
                            Controller = "Menu",
                            Action = "Index",
                            DisplayOrder = 901,
                            RequiredPermissionCode = "Menus.List",
                            IsActive = true
                        },
                        new MenuItem
                        {
                            Name = "IP Filtreleme",
                            Icon = "fas fa-shield-alt",
                            Controller = "IpFilter",
                            Action = "Index",
                            DisplayOrder = 902,
                            RequiredPermissionCode = "IpFilters.List",
                            IsActive = true
                        },
                        new MenuItem
                        {
                            Name = "Aktivite Günlüğü",
                            Icon = "fas fa-history",
                            Controller = "ActivityLog",
                            Action = "Index",
                            DisplayOrder = 903,
                            RequiredPermissionCode = "ActivityLogs.View",
                            IsActive = true
                        }
                    }
                },
            };

            await dbContext.MenuItems.AddRangeAsync(menuItems);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Menu items seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding menu items");
            throw;
        }
    }
}