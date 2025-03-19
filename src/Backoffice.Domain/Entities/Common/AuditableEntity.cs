namespace Backoffice.Domain.Entities.Common;

public abstract class AuditableEntity<TKey> : BaseEntity<TKey>, ICreationAuditableEntity, IModificationAuditableEntity
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     The date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     The user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    ///     The date and time when the entity was last modified.
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    ///     The user who last modified the entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}