using System.Linq.Expressions;
using Backoffice.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Infrastructure.Services;

public class ScheduledTaskService(
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider,
    IDbLoggerService logger)
    : IScheduledTaskService
{
    public async Task<IEnumerable<Domain.Entities.ScheduledTasks.ScheduledTask>> GetAllTasksAsync()
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();
        return await repository.GetAllAsync();
    }

    public async Task<Domain.Entities.ScheduledTasks.ScheduledTask?> GetTaskByIdAsync(int id)
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();
        return await repository.GetByIdAsync(id);
    }

    public async Task<Domain.Entities.ScheduledTasks.ScheduledTask> CreateTaskAsync(
        string taskType, Dictionary<string, string>? parameters = null)
    {
        // Task tipini çözümle
        var taskInstance = GetTaskInstance(taskType);
        if (taskInstance == null)
        {
            throw new ArgumentException($"Geçersiz task tipi: {taskType}");
        }

        // Parametreleri al, verilmemişse varsayılanları kullan
        parameters ??= taskInstance.GetDefaultParameters();

        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();

        var task = new Domain.Entities.ScheduledTasks.ScheduledTask
        {
            Name = taskInstance.Name,
            TaskType = taskType,
            Description = taskInstance.Description,
            IsActive = true,
            Interval = TimeSpan.FromHours(24), // Varsayılan 24 saat
            NextRunTime = DateTime.UtcNow,
            Parameters = parameters
        };

        await repository.AddAsync(task);
        await unitOfWork.SaveChangesAsync();

        await logger.LogInformationAsync($"Yeni scheduled task oluşturuldu: {task.Name} (ID: {task.Id})", "ScheduledTaskService");

        return task;
    }

    public async Task UpdateTaskAsync(Domain.Entities.ScheduledTasks.ScheduledTask task)
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();
        await repository.UpdateAsync(task);
        await unitOfWork.SaveChangesAsync();

        await logger.LogInformationAsync($"Scheduled task güncellendi: {task.Name} (ID: {task.Id})", "ScheduledTaskService");
    }

    public async Task<bool> ToggleTaskStatusAsync(int id)
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();
        var task = await repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        task.IsActive = !task.IsActive;
        await repository.UpdateAsync(task);
        await unitOfWork.SaveChangesAsync();

        await logger.LogInformationAsync(
            $"Scheduled task durumu değiştirildi: {task.Name} (ID: {task.Id}) - Yeni durum: {(task.IsActive ? "Aktif" : "Pasif")}",
            "ScheduledTaskService");

        return true;
    }

    public async Task<bool> RunTaskNowAsync(int id)
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();
        var task = await repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        await ExecuteTaskAsync(task);
        return true;
    }

    public async Task<IEnumerable<Domain.Entities.ScheduledTasks.TaskExecutionHistory>> GetTaskHistoryAsync(int taskId, int limit = 20)
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.TaskExecutionHistory, long>();

        var history = await repository.GetWithIncludesAsync(
            predicate: h => h.TaskId == taskId,
            orderBy: q => q.OrderByDescending(h => h.StartTime),
            includes: new List<Expression<Func<Domain.Entities.ScheduledTasks.TaskExecutionHistory, object>>>
            {
                h => h.Task
            });

        return history.Take(limit);
    }

    public async Task ExecuteTasksAsync(CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();
        var now = DateTime.UtcNow;

        var tasksToRun = await repository.GetWithIncludeStringAsync(
            predicate: t => t.IsActive && !t.IsRunning && t.NextRunTime <= now);

        foreach (var task in tasksToRun)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ExecuteTaskAsync(task, cancellationToken);
        }
    }

    private async Task ExecuteTaskAsync(Domain.Entities.ScheduledTasks.ScheduledTask task, CancellationToken cancellationToken = default)
    {
        var taskInstance = GetTaskInstance(task.TaskType);
        if (taskInstance == null)
        {
            await logger.LogErrorAsync(
                $"Task tipi çözümlenemedi: {task.TaskType}",
                "ScheduledTaskService");
            return;
        }

        var historyRepository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.TaskExecutionHistory, long>();
        var taskRepository = unitOfWork.Repository<Domain.Entities.ScheduledTasks.ScheduledTask, int>();

        var history = new Domain.Entities.ScheduledTasks.TaskExecutionHistory
        {
            TaskId = task.Id,
            StartTime = DateTime.UtcNow
        };

        await historyRepository.AddAsync(history);

        // Task'ı çalışıyor olarak işaretle
        task.IsRunning = true;
        task.LastRunTime = DateTime.UtcNow;
        await taskRepository.UpdateAsync(task);
        await unitOfWork.SaveChangesAsync();

        try
        {
            await logger.LogInformationAsync($"Task çalıştırılıyor: {task.Name} (ID: {task.Id})", "ScheduledTaskService");

            var result = await taskInstance.ExecuteAsync(task.Parameters, cancellationToken);

            history.EndTime = DateTime.UtcNow;
            history.IsSuccess = true;
            history.Result = result;

            task.LastRunResult = result;
            task.IsRunning = false;
            task.NextRunTime = DateTime.UtcNow.Add(task.Interval);

            await logger.LogInformationAsync($"Task başarıyla tamamlandı: {task.Name} (ID: {task.Id})", "ScheduledTaskService");
        }
        catch (Exception ex)
        {
            history.EndTime = DateTime.UtcNow;
            history.IsSuccess = false;
            history.ErrorMessage = $"{ex.Message}\n{ex.StackTrace}";

            task.LastRunResult = $"Hata: {ex.Message}";
            task.IsRunning = false;
            task.NextRunTime = DateTime.UtcNow.AddHours(1); // Hata durumunda 1 saat sonra tekrar dene

            await logger.LogErrorAsync(
                $"Task çalıştırılırken hata oluştu: {task.Name} (ID: {task.Id})",
                "ScheduledTaskService",
                ex);
        }

        await historyRepository.UpdateAsync(history);
        await taskRepository.UpdateAsync(task);
        await unitOfWork.SaveChangesAsync();
    }

    private IScheduledTask? GetTaskInstance(string taskType)
    {
        try
        {
            var type = Type.GetType(taskType);
            if (type == null)
            {
                return null;
            }

            return (IScheduledTask)ActivatorUtilities.CreateInstance(serviceProvider, type);
        }
        catch
        {
            return null;
        }
    }
}