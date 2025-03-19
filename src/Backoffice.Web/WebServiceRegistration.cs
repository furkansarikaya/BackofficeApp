using Backoffice.Application.Common.Interfaces;
using Backoffice.Web.Filters;
using Backoffice.Web.Services;

namespace Backoffice.Web;

public class WebServiceRegistration : IServiceRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationService, NotificationService>();
        // Permission seed service
        services.AddScoped<PermissionSeedService>();
        
        //AutoMapper
        services.AddAutoMapper(cf => cf.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        
        // Filtreler
        services.AddScoped<ExceptionFilter>();
        
        // MVC yapılandırması
        services.AddControllersWithViews(options => { options.Filters.Add<ExceptionFilter>(); });
        
        // Cookie yapılandırması
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(4);
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
        });
    }

    public int Order => 99;
}