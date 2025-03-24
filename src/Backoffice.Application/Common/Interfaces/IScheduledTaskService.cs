using Backoffice.Domain.Entities.ScheduledTasks;

namespace Backoffice.Application.Common.Interfaces;

public interface IScheduledTaskService
{
    Task<IEnumerable<ScheduledTask>> GetAllTasksAsync();
    Task<ScheduledTask?> GetTaskByIdAsync(int id);
    Task<ScheduledTask> CreateTaskAsync(string taskType, Dictionary<string, string>? parameters = null);
    Task UpdateTaskAsync(ScheduledTask task);
    Task<bool> ToggleTaskStatusAsync(int id);
    Task<bool> RunTaskNowAsync(int id);
    Task<IEnumerable<TaskExecutionHistory>> GetTaskHistoryAsync(int taskId, int limit = 20);
}