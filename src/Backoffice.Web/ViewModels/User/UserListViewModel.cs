using Backoffice.Application.Common.Models;

namespace Backoffice.Web.ViewModels.User;

public class UserListViewModel
{
    public PaginatedList<UserListItemViewModel>? Users { get; set; }
    public string? SearchTerm { get; set; }
    public bool ShowInactive { get; set; }
}