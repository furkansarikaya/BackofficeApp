using Backoffice.Application.Common.Interfaces;
using Backoffice.Infrastructure.Data;
using Backoffice.Infrastructure.Data.Interceptors;
using Backoffice.Infrastructure.Data.Repositories;
using Backoffice.Infrastructure.Identity;
using Backoffice.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backoffice.Infrastructure;

public class InfrastructureServiceRegistration : IServiceRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Interceptor'lar
        services.AddScoped<AuditableEntityInterceptor>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration
                    .GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

        // Identity yapılandırması
        services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                // Şifre politikası
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Kullanıcı politikası
                options.User.RequireUniqueEmail = true;

                // Hesap kilitleme
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Repository ve UnitOfWork
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Diğer servisler
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
    }

    public int Order => 1;
}