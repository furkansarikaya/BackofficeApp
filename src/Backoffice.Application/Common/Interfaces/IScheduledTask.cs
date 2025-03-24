namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Tüm scheduled tasklar için temel interface
/// </summary>
public interface IScheduledTask
{
    /// <summary>
    /// Task adı
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Task açıklaması
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Taskın çalıştırılması
    /// </summary>
    /// <param name="parameters">Task parametreleri</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>Çalıştırma sonucu</returns>
    Task<string> ExecuteAsync(Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Task için varsayılan parametreleri döndürür
    /// </summary>
    Dictionary<string, string> GetDefaultParameters();
}