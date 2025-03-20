using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Menu;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backoffice.Application.Services.Implementation;

public class MenuService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService,
    ILogger<MenuService> logger)
    : IMenuService
{
    public async Task<List<MenuItemDto>> GetAllMenuItemsAsync()
    {
        var repository = unitOfWork.Repository<MenuItem, int>();
        var menuItems = await repository.GetWithIncludesAsync(
            orderBy: q => q.OrderBy(m => m.DisplayOrder)
        );

        return mapper.Map<List<MenuItemDto>>(menuItems);
    }

    public async Task<List<MenuItemDto>> GetMenuHierarchyAsync()
    {
        var repository = unitOfWork.Repository<MenuItem, int>();

        // Get all menu items without includes to avoid shallow loading
        var allItems = await repository.GetWithIncludesAsync(
            orderBy: q => q.OrderBy(m => m.DisplayOrder)
        );

        // Build the hierarchy manually
        var lookup = allItems.ToLookup(i => i.ParentId);
        var rootItems = allItems.Where(m => m.ParentId == null).ToList();

        // Recursively load children
        LoadChildren(rootItems, lookup);

        // Map to DTOs
        return mapper.Map<List<MenuItemDto>>(rootItems);
    }

    private void LoadChildren(List<MenuItem> items, ILookup<int?, MenuItem> lookup)
    {
        foreach (var item in items)
        {
            var children = lookup[item.Id].ToList();
            item.Children = children;

            // Recursively load deeper levels
            if (children.Any())
            {
                LoadChildren(children, lookup);
            }
        }
    }

    public async Task<List<MenuItemDto>> GetUserMenuAsync()
    {
        var repository = unitOfWork.Repository<MenuItem, int>();

        // Get all active menu items
        var allItems = await repository.GetWithIncludesAsync(
            predicate: m => m.IsActive,
            orderBy: q => q.OrderBy(m => m.DisplayOrder),
            includes: [m => m.Children]
        );

        // Filter to get only root items (those with no parent)
        var rootItems = allItems.Where(m => m.ParentId == null).ToList();

        // Map to DTOs
        var menuDtos = mapper.Map<List<MenuItemDto>>(rootItems);

        // Check permissions for each menu item
        foreach (var menuDto in menuDtos)
        {
            // Set visibility based on permissions
            SetMenuItemVisibility(menuDto);
        }

        // Filter out items with no visible children
        FilterInvisibleMenuItems(menuDtos);

        return menuDtos;
    }

    public async Task<MenuItemDto?> GetMenuItemByIdAsync(int id)
    {
        var repository = unitOfWork.Repository<MenuItem, int>();
        var menuItem = await repository.GetByIdAsync(id);

        return menuItem == null ? null : mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<Result<int>> CreateMenuItemAsync(CreateUpdateMenuItemDto dto)
    {
        try
        {
            var repository = unitOfWork.Repository<MenuItem, int>();

            // Map DTO to entity
            var menuItem = mapper.Map<MenuItem>(dto);

            // Add to repository
            await repository.AddAsync(menuItem);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created menu item: {Name}", menuItem.Name);

            return Result<int>.Success(menuItem.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating menu item");
            return Result<int>.Failure(["Failed to create menu item: " + ex.Message]);
        }
    }

    public async Task<Result> UpdateMenuItemAsync(CreateUpdateMenuItemDto dto)
    {
        try
        {
            if (dto.Id == null)
            {
                return Result.Failure(["Menu item ID is required for update"]);
            }

            var repository = unitOfWork.Repository<MenuItem, int>();

            // Get existing menu item
            var menuItem = await repository.GetByIdAsync(dto.Id.Value);

            if (menuItem == null)
            {
                return Result.Failure([$"Menu item with ID {dto.Id} not found"]);
            }

            // Check for circular reference
            if (dto.ParentId.HasValue && dto.ParentId.Value == dto.Id.Value)
            {
                return Result.Failure(["Menu item cannot be its own parent"]);
            }

            // Update properties
            mapper.Map(dto, menuItem);

            // Update in repository
            await repository.UpdateAsync(menuItem);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated menu item: {Name} (ID: {Id})", menuItem.Name, menuItem.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating menu item");
            return Result.Failure(["Failed to update menu item: " + ex.Message]);
        }
    }

    public async Task<Result> DeleteMenuItemAsync(int id)
    {
        try
        {
            var repository = unitOfWork.Repository<MenuItem, int>();

            // Get menu item
            var menuItem = await repository.GetByIdAsync(id);

            if (menuItem == null)
            {
                return Result.Failure([$"Menu item with ID {id} not found"]);
            }

            // Check if it has children
            var childrenCount = await repository.CountAsync(m => m.ParentId == id);

            if (childrenCount > 0)
            {
                return Result.Failure(["Cannot delete menu item with children. Please delete or reassign children first."]);
            }

            // Delete from repository
            await repository.DeleteAsync(menuItem);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Deleted menu item: {Name} (ID: {Id})", menuItem.Name, menuItem.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting menu item");
            return Result.Failure(["Failed to delete menu item: " + ex.Message]);
        }
    }

    public async Task<List<MenuItemDto>> GetParentMenuItemsAsync(int? excludeId = null)
    {
        var repository = unitOfWork.Repository<MenuItem, int>();

        // Tüm menü öğelerini getir
        var allItems = await repository.GetAllAsync();

        // Hariç tutulacak ID'leri belirle
        var excludeIds = new HashSet<int>();
        if (excludeId.HasValue)
        {
            excludeIds.Add(excludeId.Value);
            var descendants = await GetDescendantIds(excludeId.Value);
            foreach (var id in descendants)
            {
                excludeIds.Add(id);
            }
        }

        // Menü hiyerarşisini oluştur
        var result = new List<MenuItem>();

        // Önce kök öğeleri al
        var rootItems = allItems
            .Where(m => m.ParentId == null && !excludeIds.Contains(m.Id))
            .OrderBy(m => m.DisplayOrder)
            .ToList();

        // Her bir kök öğe için
        foreach (var rootItem in rootItems)
        {
            // Kök öğeyi ekle
            result.Add(rootItem);

            // Alt öğeleri ekle
            AddChildrenRecursively(allItems, rootItem.Id, result, excludeIds, 1);
        }

        // DTO'lara dönüştür
        return mapper.Map<List<MenuItemDto>>(result);
    }

    // Helper methods

    private void SetMenuItemVisibility(MenuItemDto menuItem)
    {
        // Check permission
        var hasPermission = string.IsNullOrEmpty(menuItem.RequiredPermissionCode) ||
                            currentUserService.HasPermission(menuItem.RequiredPermissionCode);

        menuItem.IsVisible = hasPermission;

        // Check permissions for children
        foreach (var child in menuItem.Children)
        {
            SetMenuItemVisibility(child);
        }
    }

    private static void FilterInvisibleMenuItems(List<MenuItemDto> menuItems)
    {
        // Remove invisible non-section-header items with no visible children
        for (var i = menuItems.Count - 1; i >= 0; i--)
        {
            var item = menuItems[i];

            // Process children first (depth-first)
            FilterInvisibleMenuItems(item.Children);

            // Section headers are only visible if they have visible children
            if (item.IsSectionHeader)
            {
                item.IsVisible = item.Children.Any(c => c.IsVisible);
            }

            // Remove the item if it's not visible and has no visible children
            if (!item.IsVisible && !item.Children.Any(c => c.IsVisible))
            {
                menuItems.RemoveAt(i);
            }
        }
    }

    private async Task<List<int>> GetDescendantIds(int menuItemId)
    {
        var repository = unitOfWork.Repository<MenuItem, int>();
        var allItems = await repository.GetAllAsync();

        var descendants = new List<int>();
        var queue = new Queue<int>();
        queue.Enqueue(menuItemId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            var children = allItems.Where(m => m.ParentId == parentId).Select(m => m.Id).ToList();

            descendants.AddRange(children);

            foreach (var childId in children)
            {
                queue.Enqueue(childId);
            }
        }

        return descendants;
    }
    
    
    private static void AddChildrenRecursively(IEnumerable<MenuItem> allItems, int parentId,
        List<MenuItem> result, HashSet<int> excludeIds, int level)
    {
        if (level > 10) return; // Güvenlik sınırı

        var children = allItems
            .Where(m => m.ParentId == parentId && !excludeIds.Contains(m.Id))
            .OrderBy(m => m.DisplayOrder);

        foreach (var child in children)
        {
            // Seviyeyi göstermek için ismi değiştir - açık ve net bir girintileme sistemi
            var indent = new string('\u00A0', level * 4); // Kırılmayan boşluk karakteri
            var prefix = "";

            // Seviye göstergesini ekle
            for (var i = 0; i < level; i++)
            {
                prefix += (i == level - 1) ? "» " : "  ";
            }

            child.Name = indent + prefix + child.Name;

            // Öğeyi listeye ekle
            result.Add(child);

            // Alt öğeleri ekle
            AddChildrenRecursively(allItems, child.Id, result, excludeIds, level + 1);
        }
    }
}