using Backoffice.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.ViewComponents;
/// <summary>
/// Reusable pagination component for all list pages
/// </summary>
public class PaginationViewComponent : ViewComponent
{
    /// <summary>
    /// Creates a standardized pagination UI element
    /// </summary>
    /// <param name="model">Pagination model with all necessary data</param>
    /// <returns>View component result</returns>
    public IViewComponentResult Invoke(PaginationViewModel model)
    {
        return View(model);
    }
}

/// <summary>
/// View model for the Pagination ViewComponent
/// </summary>
public class PaginationViewModel
{
    /// <summary>
    /// Current page index (1-based)
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Page size (items per page)
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Controller name for generating links
    /// </summary>
    public string ControllerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Action method name for generating links
    /// </summary>
    public string ActionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Route values for generating links
    /// </summary>
    public Dictionary<string, object?> RouteValues { get; set; } = new();
    
    /// <summary>
    /// Maximum number of page links to show
    /// </summary>
    public int MaxDisplayedPages { get; set; } = 5;
    
    /// <summary>
    /// Whether to show "First" and "Last" page buttons
    /// </summary>
    public bool ShowFirstLast { get; set; } = true;
    
    /// <summary>
    /// Helper method to create a PaginationViewModel from a PaginatedList
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    /// <param name="paginatedList">The paginated list</param>
    /// <param name="controllerName">Controller name</param>
    /// <param name="actionName">Action method name</param>
    /// <param name="routeValues">Additional route values</param>
    /// <returns>Configured PaginationViewModel</returns>
    public static PaginationViewModel FromPaginatedList<T>(
        PaginatedList<T> paginatedList,
        string controllerName,
        string actionName, 
        Dictionary<string, object?>? routeValues = null)
    {
        return new PaginationViewModel
        {
            CurrentPage = paginatedList.PageIndex,
            TotalPages = paginatedList.TotalPages,
            TotalItems = paginatedList.TotalCount,
            PageSize = paginatedList.PageSize,
            ControllerName = controllerName,
            ActionName = actionName,
            RouteValues = routeValues ?? new Dictionary<string, object?>()
        };
    }
}