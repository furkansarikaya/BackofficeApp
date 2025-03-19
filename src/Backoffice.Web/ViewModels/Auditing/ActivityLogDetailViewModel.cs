using Backoffice.Application.DTOs.Auditing;

namespace Backoffice.Web.ViewModels.Auditing;

public class ActivityLogDetailViewModel
{
    public ActivityLogDto ActivityLog { get; set; } = null!;
    public object? DetailsObject { get; set; }
}