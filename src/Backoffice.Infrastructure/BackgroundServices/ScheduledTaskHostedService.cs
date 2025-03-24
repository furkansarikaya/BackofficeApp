using System.Collections.Concurrent;
using Backoffice.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backoffice.Infrastructure.BackgroundServices;

public class ScheduledTaskHostedService(
    IServiceProvider serviceProvider,
    ILogger<ScheduledTaskHostedService> logger)
    : BackgroundService
{
    private readonly ConcurrentDictionary<int, Timer> _taskTimers = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Scheduled Task Service başlatıldı");

        // İlk çalıştırmada tüm görevleri yükle ve timerları başlat
        await InitializeTaskTimers(stoppingToken);
        
        // Düzenli olarak (örn. 5 dakikada bir) yeni veya güncellenen görevleri kontrol et
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Görev timerları güncelleniyor");
                await SyncTaskTimers(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Görev timerları güncellenirken hata oluştu");
            }
            finally
            {
                logger.LogInformation("Görev timerları güncellenmeye devam ediyor");
            }
        }
    }

    private async Task InitializeTaskTimers(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var taskService = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();
        
        var tasks = await taskService.GetAllTasksAsync();
        
        foreach (var task in tasks)
        {
            if (task is { IsActive: true, IsRunning: false })
            {
                ScheduleTask(task);
            }
        }
    }
    
    private async Task SyncTaskTimers(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var taskService = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();
        
        var tasks = await taskService.GetAllTasksAsync();
        
        // Veritabanındaki tüm görevleri kontrol et
        foreach (var task in tasks)
        {
            // Aktif bir görev timer listesinde yoksa ekle
            if (task is { IsActive: true, IsRunning: false } && !_taskTimers.ContainsKey(task.Id))
            {
                ScheduleTask(task);
            }
            // Pasif bir görev timer listesindeyse kaldır
            else if ((!task.IsActive || task.IsRunning) && _taskTimers.ContainsKey(task.Id))
            {
                if (_taskTimers.TryRemove(task.Id, out var timer))
                {
                    await timer.DisposeAsync();
                }
            }
        }
        
        // Timer listesinde olup veritabanında olmayan görevleri temizle
        var taskIds = tasks.Select(t => t.Id).ToHashSet();
        foreach (var taskId in _taskTimers.Keys.Where(id => !taskIds.Contains(id)).ToList())
        {
            if (_taskTimers.TryRemove(taskId, out var timer))
            {
                await timer.DisposeAsync();
            }
        }
    }

    private void ScheduleTask(Domain.Entities.ScheduledTasks.ScheduledTask task)
    {
        var nextRunTime = task.NextRunTime ?? DateTime.UtcNow;
        var delay = nextRunTime > DateTime.UtcNow
            ? nextRunTime - DateTime.UtcNow
            : TimeSpan.Zero;
            
        var timer = new Timer(async _ =>
        {
            await ExecuteTaskAsync(task.Id);
            
            // Timer'ı kaldır, görev kendisini güncelleyecek
            if (_taskTimers.TryRemove(task.Id, out var removedTimer))
            {
                await removedTimer.DisposeAsync();
            }
            
            // Görev güncellenmiş olabilir, son durumunu kontrol et
            using var scope = serviceProvider.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();
            var updatedTask = await taskService.GetTaskByIdAsync(task.Id);
            
            // Görev hala aktifse, bir sonraki çalışma için yeniden zamanla
            if (updatedTask is { IsActive: true, IsRunning: false, NextRunTime: not null })
            {
                ScheduleTask(updatedTask);
            }
        }, null, delay, Timeout.InfiniteTimeSpan);
        
        _taskTimers[task.Id] = timer;
        
        logger.LogInformation(
            "Görev zamanlandı: {TaskName} (ID: {TaskId}), Bir sonraki çalışma: {NextRunTime}", 
            task.Name, task.Id, nextRunTime);
    }

    private async Task ExecuteTaskAsync(int taskId)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();
            await taskService.RunTaskNowAsync(taskId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Görev çalıştırılırken hata oluştu: {TaskId}", taskId);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Scheduled Task Service durduruluyor");
        
        // Tüm timerları temizle
        foreach (var timer in _taskTimers.Values)
        {
            timer.Dispose();
        }
        
        _taskTimers.Clear();
        
        return base.StopAsync(cancellationToken);
    }
}