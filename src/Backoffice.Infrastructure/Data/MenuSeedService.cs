using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;

namespace Backoffice.Infrastructure.Data;

/// <summary>
/// Seeds default menu items in the database
/// </summary>
public class MenuSeedService(
    ApplicationDbContext dbContext,
    IDbLoggerService logger)
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
                await logger.LogInformationAsync("Menu items already exist, skipping seeding", "DatabaseSeed");
                return;
            }

            await logger.LogInformationAsync("Seeding menu items...", "DatabaseSeed");

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
                            Icon = "",
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
                            Icon = "",
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
                            Icon = "",
                            Controller = "Menu",
                            Action = "Index",
                            DisplayOrder = 901,
                            RequiredPermissionCode = "Menus.List",
                            IsActive = true
                        },
                        new MenuItem
                        {
                            Name = "IP Filtreleme",
                            Icon = "",
                            Controller = "IpFilter",
                            Action = "Index",
                            DisplayOrder = 902,
                            RequiredPermissionCode = "IpFilters.List",
                            IsActive = true
                        },
                        new MenuItem
                        {
                            Name = "Aktivite Günlüğü",
                            Icon = "",
                            Controller = "ActivityLog",
                            Action = "Index",
                            DisplayOrder = 903,
                            RequiredPermissionCode = "ActivityLogs.List",
                            IsActive = true
                        },
                        new MenuItem
                        {
                            Name = "Loglar",
                            Icon = "",
                            Controller = "DbLog",
                            Action = "Index",
                            DisplayOrder = 904,
                            RequiredPermissionCode = "DbLogs.List",
                            IsActive = true
                        },
                        new MenuItem
                        {
                            Name = "Ayarlar",
                            Icon = "",
                            Controller = "Setting",
                            Action = "Index",
                            DisplayOrder = 904,
                            RequiredPermissionCode = "Settings.List",
                            IsActive = true
                        }
                    }
                },
            };

            await dbContext.MenuItems.AddRangeAsync(menuItems);
            await dbContext.SaveChangesAsync();

            await logger.LogInformationAsync("Menu items seed complete", "DatabaseSeed");
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync("Error seeding menu items", "DatabaseSeed", ex);
            throw;
        }
    }
}