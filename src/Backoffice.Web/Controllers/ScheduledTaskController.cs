using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Enums;
using Backoffice.Web.Attributes;
using Backoffice.Web.ViewModels.ScheduledTask;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Backoffice.Web.Controllers;

public class ScheduledTaskController(
    IScheduledTaskService taskService,
    IServiceProvider serviceProvider)
    : BaseController
{
    [Permission(PermissionType.List)]
    public async Task<IActionResult> Index()
    {
        var tasks = await taskService.GetAllTasksAsync();
        var availableTaskTypes = GetAvailableTaskTypes();

        var viewModel = new ScheduledTaskListViewModel
        {
            Tasks = tasks.ToList(),
            AvailableTaskTypes = availableTaskTypes
        };

        return View(viewModel);
    }

    [Permission(PermissionType.View)]
    public async Task<IActionResult> Details(int id)
    {
        var task = await taskService.GetTaskByIdAsync(id);

        if (task != null) return View(task);
        NotificationService.AddErrorNotification("Task bulunamadı.");
        return RedirectToAction(nameof(Index));

    }

    [Permission(PermissionType.Create)]
    public IActionResult Create()
    {
        var viewModel = new CreateTaskViewModel
        {
            AvailableTaskTypes = GetAvailableTaskTypes()
                .Select(t => new SelectListItem
                {
                    Text = GetTaskInstanceFromType(t)?.Name ?? t.Name,
                    Value = t.AssemblyQualifiedName ?? string.Empty
                })
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Create)]
    public async Task<IActionResult> Create(CreateTaskViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await taskService.CreateTaskAsync(viewModel.TaskType);
                NotificationService.AddSuccessNotification("Task başarıyla oluşturuldu.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Task oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        viewModel.AvailableTaskTypes = GetAvailableTaskTypes()
            .Select(t => new SelectListItem
            {
                Text = GetTaskInstanceFromType(t)?.Name ?? t.Name,
                Value = t.AssemblyQualifiedName ?? string.Empty,
                Selected = t.AssemblyQualifiedName == viewModel.TaskType
            })
            .ToList();

        return View(viewModel);
    }

    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await taskService.GetTaskByIdAsync(id);
        
        if (task == null)
        {
            NotificationService.AddErrorNotification("Task bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ScheduledTaskEditViewModel
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            IsActive = task.IsActive,
            IntervalHours = (int)task.Interval.TotalHours,
            IntervalMinutes = task.Interval.Minutes,
            NextRunTime = task.NextRunTime?.ToLocalTime(),
            Parameters = task.Parameters
                .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value))
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(int id, ScheduledTaskEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            NotificationService.AddErrorNotification("Geçersiz task ID.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid) return View(viewModel);
        var task = await taskService.GetTaskByIdAsync(id);
            
        if (task == null)
        {
            NotificationService.AddErrorNotification("Task bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        task.Name = viewModel.Name;
        task.Description = viewModel.Description;
        task.IsActive = viewModel.IsActive;
        task.Name = viewModel.Name;
        task.Description = viewModel.Description;
        task.IsActive = viewModel.IsActive;
        task.Interval = TimeSpan.FromHours(viewModel.IntervalHours).Add(TimeSpan.FromMinutes(viewModel.IntervalMinutes));
        task.NextRunTime = viewModel.NextRunTime?.ToUniversalTime();
        task.Parameters = task.Parameters = viewModel.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        await taskService.UpdateTaskAsync(task);
        NotificationService.AddSuccessNotification("Task başarıyla güncellendi.");
        return RedirectToAction(nameof(Index));

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await taskService.ToggleTaskStatusAsync(id);

        if (result)
            NotificationService.AddSuccessNotification("Task durumu başarıyla değiştirildi.");
        else
            NotificationService.AddErrorNotification("Task durumu değiştirilirken bir hata oluştu.");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> RunNow(int id)
    {
        var result = await taskService.RunTaskNowAsync(id);

        if (result)
            NotificationService.AddSuccessNotification("Task çalıştırılmak üzere kuyruğa alındı.");
        else
            NotificationService.AddErrorNotification("Task çalıştırılırken bir hata oluştu.");

        return RedirectToAction(nameof(Index));
    }

    [Permission(PermissionType.View)]
    public async Task<IActionResult> History(int id)
    {
        var task = await taskService.GetTaskByIdAsync(id);
        
        if (task == null)
        {
            NotificationService.AddErrorNotification("Task bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var history = await taskService.GetTaskHistoryAsync(id, 50);

        var viewModel = new ScheduledTaskHistoryViewModel
        {
            Task = task,
            History = history.ToList()
        };

        return View(viewModel);
    }

    private static List<Type> GetAvailableTaskTypes()
    {
        // IScheduledTask interface'ini implement eden tüm tipleri bul
        var taskTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IScheduledTask).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();
            
        return taskTypes;
    }

    private IScheduledTask? GetTaskInstanceFromType(Type type)
    {
        try
        {
            return (IScheduledTask)ActivatorUtilities.CreateInstance(serviceProvider, type);
        }
        catch
        {
            return null;
        }
    }
}