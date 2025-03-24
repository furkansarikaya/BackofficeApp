using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Backoffice.Web.ViewModels.ScheduledTask;

public class CreateTaskViewModel
{
    [Required]
    [Display(Name = "Task Tipi")]
    public string TaskType { get; set; } = string.Empty;
    
    public List<SelectListItem> AvailableTaskTypes { get; set; } = [];
}