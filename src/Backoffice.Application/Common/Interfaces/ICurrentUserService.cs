namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Mevcut oturum açmış kullanıcı bilgilerine erişim sağlayan arayüz
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
    string GetClientIp { get; }
    string GetUserAgent{ get; }
    string GetReferer {get; }
}