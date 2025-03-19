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
        
        // Get all menu items
        var allItems = await repository.GetWithIncludesAsync(
            orderBy: q => q.OrderBy(m => m.DisplayOrder),
            includes: [m => m.Children]
        );

        // Filter to get only root items (those with no parent)
        var rootItems = allItems.Where(m => m.ParentId == null).ToList();
        
        // Map to DTOs
        return mapper.Map<List<MenuItemDto>>(rootItems);
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
        
        // Get potential parent items (active)
        var query = repository.GetQueryable()
            .Where(m => m.IsActive);
        
        // Exclude the current item and its descendants if an ID is provided
        if (excludeId.HasValue)
        {
            // First get all descendants of the item
            var descendants = await GetDescendantIds(excludeId.Value);
            
            // Exclude the item and its descendants
            var excludeIds = new List<int> { excludeId.Value };
            excludeIds.AddRange(descendants);
            
            query = query.Where(m => !excludeIds.Contains(m.Id));
        }
        
        var items = await query
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
        
        return mapper.Map<List<MenuItemDto>>(items);
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
    
    private void FilterInvisibleMenuItems(List<MenuItemDto> menuItems)
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
}