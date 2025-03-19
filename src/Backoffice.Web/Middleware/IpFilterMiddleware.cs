using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Backoffice.Web.Middleware;

public class IpFilterMiddleware(RequestDelegate next, ILogger<IpFilterMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IIpFilterService ipFilterService,ICurrentUserService currentUserService)
    {
        var ipAddress = currentUserService.GetClientIp;

        // IP filtreleme aktif değilse veya IP'ye izin verilmişse devam et
        if (await ipFilterService.IsIpAllowedAsync(ipAddress))
        {
            await next(context);
            return;
        }

        // IP engellenmişse erişimi reddet ve özel bir sayfa göster
        logger.LogWarning("IP adresi engellendi: {IpAddress}", ipAddress);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "text/html; charset=utf-8";

        // Partial view'ı render et
        var partial = await RenderPartialViewToStringAsync("_IpAccessDenied", ipAddress, context);
        await context.Response.WriteAsync(partial);
    }

    private async Task<string> RenderPartialViewToStringAsync(string viewName, object model, HttpContext context)
    {
        // ViewEngine'e erişmek için ServiceProvider'ı kullan
        var serviceProvider = context.RequestServices;
        var razorViewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
        var tempDataProvider = serviceProvider.GetRequiredService<ITempDataProvider>();
        var stringWriter = new StringWriter();

        // View context oluştur
        var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
        var viewResult = razorViewEngine.FindView(actionContext, viewName, false);

        if (viewResult.View == null)
        {
            // View bulunamadı, temel bir hata sayfası döndür
            return $@"
                <html>
                <head>
                    <title>Erişim Engellendi</title>
                </head>
                <body>
                    <h1>Erişim Engellendi</h1>
                    <p>IP adresiniz {model} bu kaynağa erişim için yetkilendirilmemiş.</p>
                </body>
                </html>";
        }

        // View context oluştur ve render et
        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
            stringWriter,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return stringWriter.ToString();
    }
}

// Extension metodu
public static class IpFilterMiddlewareExtensions
{
    public static IApplicationBuilder UseIpFiltering(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpFilterMiddleware>();
    }
}