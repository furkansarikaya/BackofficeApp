namespace Backoffice.Domain.Enums;

/// <summary>
/// IP filtreleme türü
/// </summary>
public enum FilterType : byte
{
    /// <summary>
    /// Belirtilen IP adresine izin ver
    /// </summary>
    Allow = 0,

    /// <summary>
    /// Belirtilen IP adresini engelle
    /// </summary>
    Deny = 1
}