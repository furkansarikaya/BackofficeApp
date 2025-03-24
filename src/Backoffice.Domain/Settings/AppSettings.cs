namespace Backoffice.Domain.Settings;

public partial class AppSettings : ISettings
{
    public string SiteName { get; set; } = "Backoffice Yönetim Paneli";
    public string SiteDescription { get; set; } = "";
    public string AdminEmail { get; set; } = "";
    public int SessionTimeoutMinutes { get; set; } = 240;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int AccountLockoutMinutes { get; set; } = 15;
    public bool IpFilteringEnabled { get; set; } = true;
    public bool TwoFactorAuthEnabled { get; set; } = false;
    public string SmtpServer { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public bool SmtpUseSsl { get; set; } = true;
    public string SmtpFromEmail { get; set; } = "";
    public string SmtpFromName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public bool DebugMode { get; set; } = false;
    public string CdnUrl { get; set; } = "";
}