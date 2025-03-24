using Backoffice.Domain.Entities.Common;

namespace Backoffice.Domain.Entities.Settings;

public class Setting : AuditableEntity<int>
{
    /// <summary>
    /// Setting key in dot notation format (e.g., securitysettings.passwordexpirydays)
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Setting value as string (will be converted to appropriate type when accessed)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description for the setting
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicates if the value should be encrypted in the database
    /// </summary>
    public bool IsEncrypted { get; set; }
    
    /// <summary>
    /// The data type of the setting (string, int, bool, etc.)
    /// </summary>
    public string DataType { get; set; } = "string";
    
    /// <summary>
    /// Determines if setting can be modified through the UI
    /// </summary>
    public bool IsReadOnly { get; set; }
}