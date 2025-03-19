using System.Reflection;
using Backoffice.Application.Common.Interfaces;
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
        // services.AddScoped<ICategoryService, CategoryService>();
        // TODO: Add other services
    }

    public int Order => 2;
}