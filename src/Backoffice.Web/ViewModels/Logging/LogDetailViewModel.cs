using Backoffice.Domain.Entities.Logging;

namespace Backoffice.Web.ViewModels.Logging;

/// <summary>
/// Log detayı için view model
/// </summary>
public class LogDetailViewModel
{
    /// <summary>
    /// Log kaydı
    /// </summary>
    public LogEntry LogEntry { get; set; } = null!;
    
    /// <summary>
    /// Ek veri
    /// </summary>
    public object? AdditionalDataObject { get; set; }
}