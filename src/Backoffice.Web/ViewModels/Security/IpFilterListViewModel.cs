using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Security;

namespace Backoffice.Web.ViewModels.Security;

public class IpFilterListViewModel
{
    public PaginatedList<IpFilterDto>? IpFilters { get; set; }
    public string? SearchTerm { get; set; }
}