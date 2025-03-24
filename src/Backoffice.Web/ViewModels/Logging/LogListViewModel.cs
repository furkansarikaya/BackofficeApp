using Backoffice.Application.Common.Models;
using Backoffice.Domain.Entities.Logging;

namespace Backoffice.Web.ViewModels.Logging;

/// <summary>
/// Log listesi için view model
/// </summary>
public class LogListViewModel
{
    /// <summary>
    /// Log kayıtları
    /// </summary>
    public PaginatedList<LogEntry>? Logs { get; set; }
    
    /// <summary>
    /// Filtre
    /// </summary>
    public LogFilterViewModel Filter { get; set; } = new();
    
    /// <summary>
    /// Tüm kategoriler
    /// </summary>
    public List<string> Categories { get; set; } = new();
}