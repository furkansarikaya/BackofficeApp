using Backoffice.Domain.Enums;

namespace Backoffice.Application.DTOs.Security;

public class CreateUpdateIpFilterDto
{
    public int? Id { get; set; } // Oluşturmada null, güncellemede değer içerir
    public string IpAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FilterType FilterType { get; set; }
    public bool IsActive { get; set; } = true;
}