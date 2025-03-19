using Backoffice.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;

namespace Backoffice.Infrastructure.Identity;

public class ApplicationUser : IdentityUser,ICreationAuditableEntity,IModificationAuditableEntity,ISoftDelete
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}