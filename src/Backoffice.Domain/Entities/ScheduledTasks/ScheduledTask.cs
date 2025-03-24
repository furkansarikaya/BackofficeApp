using Backoffice.Domain.Entities.Common;

namespace Backoffice.Domain.Entities.ScheduledTasks;

public class ScheduledTask : AuditableEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty; // Assembly qualified name
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24); // Default 24 saat
    public DateTime? LastRunTime { get; set; }
    public DateTime? NextRunTime { get; set; }
    public bool IsRunning { get; set; } = false;
    public string? LastRunResult { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    
    // JSON olarak serileştirilen parametreler için accessor property
    public string ParametersJson
    {
        get => System.Text.Json.JsonSerializer.Serialize(Parameters);
        set => Parameters = !string.IsNullOrEmpty(value) 
            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new()
            : new();
    }
}