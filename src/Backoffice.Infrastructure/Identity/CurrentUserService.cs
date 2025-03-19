using System.Security.Claims;
using Backoffice.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Backoffice.Infrastructure.Identity;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IIdentityService identityService)
    : ICurrentUserService
{
    public string UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1";
    
    public string UserName => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? "System";
    
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public bool IsInRole(string role)
    {
        return httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
    
    public bool HasPermission(string permission)
    {
        return IsAuthenticated &&
               // Asenkron metodu senkron çağırmak için Wait/Result kullanımı
               identityService.HasPermissionAsync(UserId, permission).GetAwaiter().GetResult();
    }

    public string GetClientIp
    {
        get
        {

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
                return "0.0.0.0";

            // 1. Proxy arkası kontrolü (X-Forwarded-For)
            var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
            {
                // 2. Alternatif Proxy başlığı (Nginx, AWS gibi sistemlerde olabilir)
                ip = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(ip))
            {
                // 3. Doğrudan bağlanan istemci IP'si
                ip = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            if (string.IsNullOrEmpty(ip))
                return "0.0.0.0";

            // 4. Eğer IPv6 loopback (::1) ise, bunu IPv4 loopback (127.0.0.1) olarak değiştir
            return ip == "::1" ? "0.0.0.0" : ip;
        }
    }

    public string GetUserAgent => httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    public string GetReferer => httpContextAccessor.HttpContext?.Request.Headers["Referer"].ToString() ?? "Unknown";
}