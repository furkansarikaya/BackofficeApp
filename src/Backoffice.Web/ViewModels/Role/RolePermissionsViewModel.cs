namespace Backoffice.Web.ViewModels.Role;

public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ControllerPermissionsViewModel> ControllerGroups { get; set; } = new();
}

public class ControllerPermissionsViewModel
{
    public string ControllerName { get; set; } = string.Empty;
    public List<PermissionViewModel> Permissions { get; set; } = new();
}

public class PermissionViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}