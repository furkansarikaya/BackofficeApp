namespace Backoffice.Application.DTOs.Auditing;

public class CreateActivityLogDto
{
    public string Category { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
}