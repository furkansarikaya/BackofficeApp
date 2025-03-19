using Backoffice.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;

namespace Backoffice.Infrastructure.Identity;

public class ApplicationRole : IdentityRole,ICreationAuditableEntity,IModificationAuditableEntity,ISoftDelete
{
    public ApplicationRole() { }
    
    public ApplicationRole(string roleName) : base(roleName) { }
    
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}