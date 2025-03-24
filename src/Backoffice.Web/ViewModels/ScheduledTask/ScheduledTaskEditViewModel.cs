namespace Backoffice.Web.ViewModels.ScheduledTask;

public class ScheduledTaskEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int IntervalHours { get; set; }
    public int IntervalMinutes { get; set; }
    public DateTime? NextRunTime { get; set; }
    public List<KeyValuePair<string, string>> Parameters { get; set; } = [];
}