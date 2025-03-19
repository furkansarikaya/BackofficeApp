namespace Backoffice.Domain.Exceptions;

public class EntityNotFoundException(string entityName, object entityId) : Exception($"{entityName} with id {entityId} was not found.")
{
    public string EntityName { get; } = entityName;
    public object EntityId { get; } = entityId;
}