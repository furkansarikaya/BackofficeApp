namespace Backoffice.Application.DTOs.Auditing;

public class ActivityLogFilterDto
{
    public string? UserId { get; set; }
    public string? Category { get; set; }
    public string? ActivityType { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}