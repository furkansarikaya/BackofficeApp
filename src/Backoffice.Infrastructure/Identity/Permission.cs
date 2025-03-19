using Backoffice.Domain.Entities.Common;

namespace Backoffice.Infrastructure.Identity;

public class Permission : BaseEntity<int>
{
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}