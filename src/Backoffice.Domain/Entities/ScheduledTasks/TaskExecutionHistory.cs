using Backoffice.Domain.Entities.Common;

namespace Backoffice.Domain.Entities.ScheduledTasks;

public class TaskExecutionHistory : BaseEntity<long>
{
    public int TaskId { get; set; }
    public ScheduledTask Task { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
}