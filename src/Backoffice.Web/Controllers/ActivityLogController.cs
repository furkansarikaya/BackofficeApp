using System.Text.Json;
using Backoffice.Application.DTOs.Auditing;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Enums;
using Backoffice.Infrastructure.Identity;
using Backoffice.Web.Attributes;
using Backoffice.Web.ViewModels.Auditing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.Controllers;

public class ActivityLogController(
    IActivityLogService activityLogService,
    UserManager<ApplicationUser> userManager)
    : BaseController
{
    // GET: /ActivityLog
    [Permission(PermissionType.View)]
    public async Task<IActionResult> Index(ActivityLogFilterViewModel filter, int pageNumber = 1)
    {
        // DTO'ya dönüştür
        var filterDto = new ActivityLogFilterDto
        {
            UserId = filter.UserId,
            Category = filter.Category,
            ActivityType = filter.ActivityType,
            EntityType = filter.EntityType,
            EntityId = filter.EntityId,
            SearchTerm = filter.SearchTerm,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate
        };
        
        // Verileri getir
        var logs = await activityLogService.GetActivityLogsAsync(filterDto, pageNumber, 20);
        
        // Kategorileri getir
        var categories = activityLogService.GetAllCategories();
        
        // Aktivite tiplerini getir
        var activityTypes = !string.IsNullOrEmpty(filter.Category) 
            ? activityLogService.GetActivityTypesByCategory(filter.Category) 
            : new List<string>();
        
        // ViewModel oluştur
        var viewModel = new ActivityLogListViewModel
        {
            ActivityLogs = logs,
            Filter = filter,
            Categories = categories,
            ActivityTypes = activityTypes
        };
        
        return View(viewModel);
    }

    // GET: /ActivityLog/Details/5
    [Permission(PermissionType.View)]
    public async Task<IActionResult> Details(long id)
    {
        var log = await activityLogService.GetActivityLogByIdAsync(id);
        
        if (log == null)
        {
            NotificationService.AddErrorNotification("Aktivite logu bulunamadı.");
            return RedirectToAction(nameof(Index));
        }
        
        // Detay JSON verisini objeye dönüştür
        object? detailsObject = null;
        if (!string.IsNullOrEmpty(log.Details))
        {
            try
            {
                detailsObject = JsonSerializer.Deserialize<Dictionary<string, object>>(log.Details);
            }
            catch
            {
                // Parse hatası olursa JSON string'i olduğu gibi göster
                detailsObject = log.Details;
            }
        }
        
        var viewModel = new ActivityLogDetailViewModel
        {
            ActivityLog = log,
            DetailsObject = detailsObject
        };
        
        return View(viewModel);
    }

    // GET: /ActivityLog/UserActivities/userId
    [Permission(PermissionType.View)]
    public async Task<IActionResult> UserActivities(string userId, int pageNumber = 1)
    {
        var user = await userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
            return RedirectToAction(nameof(Index));
        }
        
        var logs = await activityLogService.GetUserActivityLogsAsync(userId, pageNumber, 20);
        
        // Kategorileri getir
        var categories = activityLogService.GetAllCategories();
        
        // ViewModel oluştur
        var viewModel = new ActivityLogListViewModel
        {
            ActivityLogs = logs,
            Filter = new ActivityLogFilterViewModel { UserId = userId },
            Categories = categories
        };
        
        ViewData["UserName"] = user.UserName;
        
        return View("Index", viewModel);
    }

    // GET: /ActivityLog/EntityActivities?entityType=Menu&entityId=5
    [Permission(PermissionType.View)]
    public async Task<IActionResult> EntityActivities(string entityType, string entityId, int pageNumber = 1)
    {
        if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId))
        {
            NotificationService.AddErrorNotification("Varlık tipi ve ID gereklidir.");
            return RedirectToAction(nameof(Index));
        }
        
        // Filtreyi oluştur
        var filterDto = new ActivityLogFilterDto
        {
            EntityType = entityType,
            EntityId = entityId
        };
        
        // Verileri getir
        var logs = await activityLogService.GetActivityLogsAsync(filterDto, pageNumber, 20);
        
        // Kategorileri getir
        var categories = activityLogService.GetAllCategories();
        
        // ViewModel oluştur
        var viewModel = new ActivityLogListViewModel
        {
            ActivityLogs = logs,
            Filter = new ActivityLogFilterViewModel 
            { 
                EntityType = entityType,
                EntityId = entityId
            },
            Categories = categories
        };
        
        ViewData["EntityName"] = $"{entityType} (ID: {entityId})";
        
        return View("Index", viewModel);
    }

    // AJAX: /ActivityLog/GetActivityTypes?category=UserManagement
    [HttpGet]
    public IActionResult GetActivityTypes(string category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return Json(new List<string>());
        }
        
        var activityTypes = activityLogService.GetActivityTypesByCategory(category);
        
        return Json(activityTypes);
    }
    
    // GET: /ActivityLog/UserActivitiesPartial?userId=abc123
    [HttpGet]
    public async Task<IActionResult> UserActivitiesPartial(string userId)
    {
        var logs = await activityLogService.GetUserActivityLogsAsync(userId, 1, 10);
        return PartialView("_UserActivitiesPartial", logs);
    }
}