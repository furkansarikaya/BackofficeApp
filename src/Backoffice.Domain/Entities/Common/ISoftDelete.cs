namespace Backoffice.Domain.Entities.Common;

public interface ISoftDelete
{
    /// <summary>
    ///     Indicates whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    ///     The date and time when the entity was deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    ///     The user who deleted the entity.
    /// </summary>
    string? DeletedBy { get; set; }
}