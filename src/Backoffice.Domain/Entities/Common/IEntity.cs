namespace Backoffice.Domain.Entities.Common;

public interface IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     The unique identifier for the entity.
    /// </summary>
    TKey Id { get; set; }
}