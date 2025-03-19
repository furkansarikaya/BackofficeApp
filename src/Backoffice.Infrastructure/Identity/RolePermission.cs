using Backoffice.Domain.Entities.Common;

namespace Backoffice.Infrastructure.Identity;

public class RolePermission: AuditableEntity<int>
{
    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole Role { get; set; } = null!;
    public string PermissionCode { get; set; } = string.Empty;
}