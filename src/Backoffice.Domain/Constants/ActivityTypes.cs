namespace Backoffice.Domain.Constants;

/// <summary>
/// Aktivite günlüğü için aktivite tipleri sabitleri
/// </summary>
public static class ActivityTypes
{
    // Genel aktiviteler
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string FailedLogin = "FailedLogin";
    public const string PasswordChanged = "PasswordChanged";
    public const string PasswordReset = "PasswordReset";
    
    // Veri işlemleri
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string View = "View";
    public const string Export = "Export";
    public const string Import = "Import";
    
    // Yönetim işlemleri
    public const string GrantPermission = "GrantPermission";
    public const string RevokePermission = "RevokePermission";
    public const string ChangeStatus = "ChangeStatus";
    public const string ChangeRole = "ChangeRole";
    
    // Sistem işlemleri
    public const string SystemStartup = "SystemStartup";
    public const string SystemShutdown = "SystemShutdown";
    public const string ConfigChange = "ConfigChange";
    public const string BackupCreate = "BackupCreate";
    public const string BackupRestore = "BackupRestore";
}