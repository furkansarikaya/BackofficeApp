namespace Backoffice.Web.ViewModels.ScheduledTask;

public class ScheduledTaskListViewModel
{
    public List<Domain.Entities.ScheduledTasks.ScheduledTask> Tasks { get; set; } = [];
    public List<Type> AvailableTaskTypes { get; set; } = [];
}