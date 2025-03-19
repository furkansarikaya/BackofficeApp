using Backoffice.Domain.Entities.Common;
using Backoffice.Domain.Enums;

namespace Backoffice.Domain.Entities.Security;

public class IpFilter : AuditableEntity<int>
{
    /// <summary>
    /// IP adresi veya CIDR notasyonu (örn: 192.168.1.1 veya 192.168.1.0/24)
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// IP adresi açıklaması (örn: "Şirket Ofisi")
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Bu IP için filtreleme türü (izin verme veya engelleme)
    /// </summary>
    public FilterType FilterType { get; set; }
    
    /// <summary>
    /// IP filtresinin aktif olup olmadığı
    /// </summary>
    public bool IsActive { get; set; } = true;
}