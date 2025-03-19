using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Modüler servis kaydı için arayüz. Tüm modüller bu arayüzü uygulamalıdır.
/// </summary>
public interface IServiceRegistration
{
    /// <summary>
    /// Modüle ait servisleri IServiceCollection'a ekler
    /// </summary>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    
    /// <summary>
    ///  Modülün yüklenme sırası. Daha düşük sayılar önce yüklenir.
    /// </summary>
    int Order { get; }
}