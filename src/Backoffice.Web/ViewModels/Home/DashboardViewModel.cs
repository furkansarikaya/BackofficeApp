using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Auditing;

namespace Backoffice.Web.ViewModels.Home;

public class DashboardViewModel
{
    // User statistics
    public UserStatistics? UserStatistics { get; set; }
    
    // Activity data
    public PaginatedList<ActivityLogDto>? RecentActivities { get; set; }
}

public class UserStatistics(int totalUsers, int activeUsers, decimal userIncrease)
{
    public int TotalUsers { get; set; } = totalUsers;
    public int ActiveUsers { get; set; } = activeUsers;
    public decimal UserIncrease { get; set; } = userIncrease;
}