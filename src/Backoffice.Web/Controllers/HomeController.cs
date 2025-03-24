using System.Diagnostics;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.DTOs.Auditing;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;
using Backoffice.Web.Models;
using Backoffice.Web.ViewModels.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backoffice.Web.Controllers;

public class HomeController(UserManager<ApplicationUser> userManager,IActivityLogService activityLogService,IDateTimeService dateTimeService) : BaseController
{
    public async Task<IActionResult> Index()
    {
        // Get total users count including active and inactive users
        var totalUsers = await userManager.Users.CountAsync();
        
        // Get count of active users
        var activeUsers = await userManager.Users.CountAsync(u => u.IsActive);
        
        // Calculate UserIncrease - users created in current month vs previous month
        var today = dateTimeService.Now;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        
        var currentMonthUsers = await userManager.Users
            .CountAsync(u => u.CreatedAt >= currentMonthStart);
            
        var previousMonthUsers = await userManager.Users
            .CountAsync(u => u.CreatedAt >= previousMonthStart && u.CreatedAt < currentMonthStart);
        
        // Calculate percentage increase
        decimal userIncrease = 0;
        if (previousMonthUsers > 0)
        {
            userIncrease = ((decimal)currentMonthUsers - previousMonthUsers) / previousMonthUsers * 100;
        }
        else if (currentMonthUsers > 0)
        {
            userIncrease = 100; // If there were no users last month but there are this month, it's a 100% increase
        }
        
        // Get recent activities
        var activityFilter = new ActivityLogFilterDto
        {
            // No filters, get latest activities
        };
        var recentActivities = await activityLogService.GetActivityLogsAsync(activityFilter, 1, 5);
        
        // Prepare the dashboard view model
        var viewModel = new DashboardViewModel
        {
            UserStatistics = new UserStatistics(totalUsers, activeUsers, Math.Round(userIncrease, 1)),
            RecentActivities = recentActivities
        };
        
        return View(viewModel);
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}