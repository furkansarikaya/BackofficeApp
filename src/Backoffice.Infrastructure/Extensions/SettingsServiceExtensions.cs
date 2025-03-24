using System.Reflection;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Infrastructure.Extensions;

public static class SettingsServiceExtensions
{
    public static IServiceCollection AddAllSettings(this IServiceCollection services)
    {
        //Tüm ayarları yükle
        var settingsService = services.BuildServiceProvider().GetRequiredService<ISettingsService>();
        settingsService.GetAllSettingsAsync().GetAwaiter().GetResult();

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

        // ISettings'i uygulayan tüm concrete sınıfları bul
        var settingsTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISettings).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToList();

        // Her bir ISettings tipini register et
        foreach (var settingsType in settingsTypes)
        {
            // Generic AddSettingType metodunu reflection ile çağır
            typeof(SettingsServiceExtensions)
                .GetMethod(nameof(AddSettingType), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(settingsType)
                .Invoke(null, new object[] { services });
        }

        return services;
    }

    // Generic helper metodu - reflection ile çağrılacak
    private static void AddSettingType<T>(IServiceCollection services) where T : class, ISettings, new()
    {
        // Singleton olarak kaydet
        services.AddScoped(_ =>
        {
            var settingsService = services.BuildServiceProvider().GetRequiredService<ISettingsService>();
            
            // Ayarları veritabanından yükle
            var settings = new T();
            settingsService.BindSettingsAsync(settings).GetAwaiter().GetResult();
            return settings;
        });
    }
}