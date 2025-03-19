using AutoMapper;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Web.ViewModels.Menu;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.ViewComponents;

public class MenuViewComponent(
    IMenuService menuService,
    IMapper mapper,
    ILogger<MenuViewComponent> logger)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            // Get menu items that the current user has permission to see
            var menuItems = await menuService.GetUserMenuAsync();
            
            // Map to view models
            var viewModels = mapper.Map<List<MenuViewModel>>(menuItems);
            
            // Process all menu items
            ProcessMenuItems(viewModels);
            
            return View(viewModels);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading menu items");
            return View(new List<MenuViewModel>());
        }
    }

    private void ProcessMenuItems(List<MenuViewModel> menuItems)
    {
        // Get current controller and action
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();
        
        // First identify which section contains the current page
        var activeSectionFound = false;
        
        foreach (var item in from item in menuItems where item.IsSectionHeader let sectionHasCurrentPage = CheckSectionForCurrentPage(item.Children, currentController, currentAction) where sectionHasCurrentPage select item)
        {
            item.IsExpanded = true;
            activeSectionFound = true;
        }
        
        // If no section contains the current page, expand all sections by default
        if (!activeSectionFound)
        {
            foreach (var item in menuItems.Where(item => item.IsSectionHeader))
            {
                item.IsExpanded = true;
            }
        }
        
        // Now process all menu items to mark current page
        ProcessMenuItemsRecursive(menuItems, currentController, currentAction);
    }
    
    private static bool CheckSectionForCurrentPage(List<MenuViewModel> items, string? currentController, string? currentAction)
    {
        foreach (var item in items)
        {
            if (string.Equals(item.Controller, currentController, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Action, currentAction, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            if (item.Children.Count != 0 && CheckSectionForCurrentPage(item.Children, currentController, currentAction))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private static bool ProcessMenuItemsRecursive(List<MenuViewModel> menuItems, string? currentController, string? currentAction)
    {
        var hasCurrentPage = false;
        
        foreach (var item in menuItems)
        {
            // Skip section headers for is-current-page check but still process children
            if (!item.IsSectionHeader)
            {
                // Check if this is the current page
                if (string.Equals(item.Controller, currentController, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.Action, currentAction, StringComparison.OrdinalIgnoreCase))
                {
                    item.IsCurrentPage = true;
                    item.IsExpanded = true; // Expand this item if it's current
                    hasCurrentPage = true;
                }
            }
            
            // Process children recursively
            if (item.Children.Count == 0) continue;
            var childHasCurrentPage = ProcessMenuItemsRecursive(item.Children, currentController, currentAction);
                
            // If any child is current, this item is also expanded
            if (!childHasCurrentPage) continue;
            item.IsExpanded = true;
            hasCurrentPage = true;
        }
        
        return hasCurrentPage;
    }
}