using System.Reflection;
using Backoffice.Application.Common.Interfaces;

namespace Backoffice.Web.Extensions;

/// <summary>
/// IServiceRegistration arayüzünü uygulayan tüm tipleri otomatik olarak bulup kaydeden yardımcı sınıf
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Tüm assembly'lerde IServiceRegistration arayüzünü uygulayan sınıfları bulur ve RegisterServices metodlarını çağırır
    /// </summary>
    public static void AddServiceRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        // Çalışan uygulamadaki tüm assembly'leri al
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        // Henüz yüklenmemiş assembly'leri yükle
        var referencedAssemblies = Assembly.GetEntryAssembly()?
            .GetReferencedAssemblies()
            .Where(a => assemblies.All(loaded => loaded.GetName().Name != a.Name))
            .Select(Assembly.Load)
            .ToList();

        if (referencedAssemblies != null)
        {
            assemblies.AddRange(referencedAssemblies);
        }  
        
        // IServiceRegistration arayüzünü uygulayan tüm tipleri bul
        var serviceRegistrationTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsInterface: false, IsAbstract: false } && typeof(IServiceRegistration).IsAssignableFrom(t))
            .ToList();

        // Her bir tip için instance oluştur ve RegisterServices metodunu çağır
        foreach (var registration in serviceRegistrationTypes.Select(type => Activator.CreateInstance(type) as IServiceRegistration).OrderBy(r => r?.Order))
        {
            registration?.RegisterServices(services, configuration);
        }
    }
}