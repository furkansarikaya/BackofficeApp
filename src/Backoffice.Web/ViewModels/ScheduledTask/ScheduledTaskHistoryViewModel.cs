namespace Backoffice.Web.ViewModels.ScheduledTask;

public class ScheduledTaskHistoryViewModel
{
    public Domain.Entities.ScheduledTasks.ScheduledTask Task { get; set; } = null!;
    public List<Domain.Entities.ScheduledTasks.TaskExecutionHistory> History { get; set; } = new();
}