using System.Reflection;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Services.Implementation;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Application.Tasks;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Application;

public class ApplicationServiceRegistration : IServiceRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Services
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IIpFilterService, IpFilterService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        
        
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
        
        // IScheduledTask interface'ini implemente eden tüm tipleri bulup otomatik kaydet
        var scheduledTaskTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsInterface: false, IsAbstract: false } && typeof(IScheduledTask).IsAssignableFrom(t));
            
        foreach (var taskType in scheduledTaskTypes)
        {
            services.AddTransient(typeof(IScheduledTask), taskType);
        }
    }

    public int Order => 2;
}