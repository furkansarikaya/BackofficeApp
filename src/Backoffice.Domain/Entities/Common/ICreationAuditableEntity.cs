namespace Backoffice.Domain.Entities.Common;

public interface ICreationAuditableEntity
{
    /// <summary>
    ///     The date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    ///     The user who created the entity.
    /// </summary>
    string CreatedBy { get; set; }
}