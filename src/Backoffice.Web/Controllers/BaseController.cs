using Backoffice.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backoffice.Web.Controllers;

/// <summary>
/// Tüm controller'lar için temel sınıf.
/// Ortak davranışları ve özellikleri içerir.
/// </summary>
public abstract class BaseController : Controller
{
    private INotificationService? _notificationService;
    protected INotificationService NotificationService => _notificationService ??= HttpContext.RequestServices.GetRequiredService<INotificationService>();

    /// <summary>
    /// Her action çalıştırılmadan önce çağrılır.
    /// Kullanıcının kimlik doğrulamasını kontrol eder.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Kullanıcı giriş yapmış mı kontrol et
        if (!User.Identity.IsAuthenticated)
        {
            // PermissionAttribute ile korunan action'lar zaten kimlik doğrulama gerektirir,
            // bu kontrol daha çok ek güvenlik kontrolü olarak hizmet eder
            // veya [AllowAnonymous] ile işaretlenmiş action'lar dışındaki tüm action'ların
            // giriş gerektirmesini sağlar
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            
            if (allowAnonymous == null)
            {
                // Kullanıcı giriş yapmamış ve action anonim değil
                NotificationService.AddWarningNotification("Bu sayfayı görüntülemek için giriş yapmalısınız.");
                
                // Giriş sayfasına yönlendir ve geri dönüş URL'ini kaydet
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }
        }

        base.OnActionExecuting(context);
    }

    /// <summary>
    /// Geçerli URL ile RedirectToAction döndürür.
    /// Form submit edildikten sonra aynı sayfaya dönmek için kullanışlıdır.
    /// </summary>
    protected IActionResult RedirectToCurrentAction()
    {
        return Redirect(Request.Path + Request.QueryString);
    }
}