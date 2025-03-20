using Backoffice.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backoffice.Web.Filters;

/// <summary>
/// İzin kontrolü yapan filtre
/// </summary>
public class PermissionFilter(string permissionCode) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // CurrentUserService al
        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        
        // Admin rolü kontrolü - her zaman tüm izinlere sahiptir
        if (currentUserService.IsInRole("Administrator"))
        {
            await next();
            return;
        }
        
        // Kullanıcı giriş yapmış mı?
        if (!currentUserService.IsAuthenticated)
        {
            context.Result = new ChallengeResult();
            return;
        }
        
        if(currentUserService.IsInRole("Administrator"))
        {
            await next();
            return;
        }
        
        // İzin kontrolü
        if (!currentUserService.HasPermission(permissionCode))
        {
            if (context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                context.Result = new PartialViewResult
                {
                    ViewName = "~/Views/Account/AccessDenied.cshtml",
                };
            }
            else
                context.Result = new ForbidResult();

            return;
        }
        
        // Kullanıcı yetkiliyse işleme devam et
        await next();
    }
}