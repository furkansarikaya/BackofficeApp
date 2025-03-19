using Backoffice.Domain.Enums;

namespace Backoffice.Application.DTOs.Security;

public class IpFilterDto
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FilterType FilterType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}