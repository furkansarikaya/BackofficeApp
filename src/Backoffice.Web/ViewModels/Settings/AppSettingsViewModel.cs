using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Backoffice.Web.Attributes;

namespace Backoffice.Web.ViewModels.Settings;

/// <summary>
/// Application settings view model that maps to setting keys with dotted notation
/// </summary>
public class AppSettingsViewModel
{
    [Display(Name = "Site Adı")]
    [Description("Sitenin başlık kısmında görünecek isim")]
    public string SiteName { get; set; } = "Backoffice Yönetim Paneli";
    
    [Display(Name = "Site Açıklaması")]
    [Description("Meta açıklama için kullanılır")]
    public string? SiteDescription { get; set; } = "";
    
    [Display(Name = "Admin E-posta")]
    [Description("Sistem bildirimleri için kullanılacak e-posta adresi")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string? AdminEmail { get; set; } = "";
    
    [Display(Name = "Oturum Süresi (Dakika)")]
    [Description("Kullanıcı oturumunun ne kadar süre aktif kalacağı")]
    [Range(5, 1440, ErrorMessage = "Oturum süresi 5-1440 dakika arasında olmalıdır.")]
    public int SessionTimeoutMinutes { get; set; } = 240;
    
    [Display(Name = "Maksimum Hatalı Giriş")]
    [Description("Hesap kilitlenmeden önce izin verilen maksimum hatalı giriş sayısı")]
    [Range(1, 10, ErrorMessage = "Hatalı giriş sayısı 1-10 arasında olmalıdır.")]
    public int MaxFailedLoginAttempts { get; set; } = 5;
    
    [Display(Name = "Hesap Kilitleme Süresi (Dakika)")]
    [Description("Çok sayıda hatalı giriş yapıldığında hesabın kilitli kalacağı süre")]
    [Range(5, 1440, ErrorMessage = "Kilitleme süresi 5-1440 dakika arasında olmalıdır.")]
    public int AccountLockoutMinutes { get; set; } = 15;
    
    [Display(Name = "IP Filtreleme Aktif")]
    [Description("IP filtreleme özelliğini aktif/pasif yapar")]
    public bool IpFilteringEnabled { get; set; } = true;
    
    [Display(Name = "İki Faktörlü Kimlik Doğrulama")]
    [Description("İki faktörlü kimlik doğrulamayı etkinleştirir")]
    public bool TwoFactorAuthEnabled { get; set; } = false;
    
    [Display(Name = "SMTP Sunucu")]
    [Description("E-posta göndermek için kullanılacak SMTP sunucusu")]
    public string? SmtpServer { get; set; } = "";
    
    [Display(Name = "SMTP Port")]
    [Description("SMTP sunucusu port numarası")]
    [Range(1, 65535, ErrorMessage = "Port numarası 1-65535 arasında olmalıdır.")]
    public int SmtpPort { get; set; } = 587;
    
    [Display(Name = "SMTP Kullanıcı Adı")]
    [Description("SMTP kimlik doğrulaması için kullanıcı adı")]
    public string? SmtpUsername { get; set; } = "";
    
    [Display(Name = "SMTP Şifre")]
    [Description("SMTP kimlik doğrulaması için şifre")]
    [Encrypt]
    public string? SmtpPassword { get; set; } = "";
    
    [Display(Name = "SMTP SSL")]
    [Description("SMTP bağlantısında SSL kullanımını etkinleştirir")]
    public bool SmtpUseSsl { get; set; } = true;
    
    [Display(Name = "SMTP Gönderen")]
    [Description("E-posta gönderirken kullanılacak gönderen adresi")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string? SmtpFromEmail { get; set; } = "";
    
    [Display(Name = "SMTP Gönderen Adı")]
    [Description("E-posta gönderirken kullanılacak gönderen adı")]
    public string? SmtpFromName { get; set; } = "";
    
    [Display(Name = "API Anahtarı")]
    [Description("Dış sistemlerle entegrasyon için API anahtarı")]
    [Encrypt]
    public string? ApiKey { get; set; } = "";
    
    [Display(Name = "Debug Modu")]
    [Description("Hata ayıklama modunu etkinleştirir (yalnızca geliştirme ortamında kullanın)")]
    public bool DebugMode { get; set; } = false;
    
    [Display(Name = "CDN URL")]
    [Description("Statik dosyalar için CDN URL (boş bırakılırsa yerel sunucu kullanılır)")]
    public string? CdnUrl { get; set; } = "";
}