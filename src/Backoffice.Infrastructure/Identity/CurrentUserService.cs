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
}