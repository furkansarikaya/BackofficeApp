using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Menu;

namespace Backoffice.Application.Services.Interfaces;

public interface IMenuService : IGenericService<int, MenuItemDto, CreateUpdateMenuItemDto>
{
    /// <summary>
    /// Gets menu items as a hierarchical structure
    /// </summary>
    Task<List<MenuItemDto>> GetMenuHierarchyAsync();
    
    /// <summary>
    /// Gets menu items that the current user has permission to see
    /// </summary>
    Task<List<MenuItemDto>> GetUserMenuAsync();
    
    /// <summary>
    /// Gets a menu item by ID
    /// </summary>
    Task<MenuItemDto?> GetMenuItemByIdAsync(int id);
    
    /// <summary>
    /// Gets a list of menu items that can be parents (for dropdown lists)
    /// </summary>
    Task<List<MenuItemDto>> GetParentMenuItemsAsync(int? excludeId = null);
}