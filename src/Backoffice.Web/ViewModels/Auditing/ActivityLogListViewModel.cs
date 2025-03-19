using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Auditing;

namespace Backoffice.Web.ViewModels.Auditing;

public class ActivityLogListViewModel
{
    public PaginatedList<ActivityLogDto>? ActivityLogs { get; set; }
    public ActivityLogFilterViewModel Filter { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> ActivityTypes { get; set; } = new();
}