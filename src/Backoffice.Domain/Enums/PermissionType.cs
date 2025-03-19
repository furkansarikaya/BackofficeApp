namespace Backoffice.Domain.Enums;

/// <summary>
/// İzin tipleri - Controller eylemlerine karşılık gelir
/// </summary>
public enum PermissionType : byte
{
    List,
    View,
    Create,
    Update,
    Delete
}