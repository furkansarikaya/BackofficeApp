namespace Backoffice.Domain.Entities.Common;

public interface IModificationAuditableEntity
{
    /// <summary>
    ///     The date and time when the entity was last modified.
    /// </summary>
    DateTime? LastModifiedAt { get; set; }

    /// <summary>
    ///     The user who last modified the entity.
    /// </summary>
    string? LastModifiedBy { get; set; }
}