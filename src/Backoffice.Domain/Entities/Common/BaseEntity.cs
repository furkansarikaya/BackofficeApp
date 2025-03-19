namespace Backoffice.Domain.Entities.Common;

public abstract class BaseEntity<TKey> : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     The unique identifier for the entity.
    /// </summary>
    public TKey Id { get; set; } = default!;
}