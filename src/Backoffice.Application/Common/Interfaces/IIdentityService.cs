using Backoffice.Application.Common.Models;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Kimlik doğrulama ve yetkilendirme işlemleri için servis arayüzü
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Kullanıcı ID'sine göre kullanıcı adını getirir
    /// </summary>
    Task<string?> GetUserNameAsync(string userId);
    
    /// <summary>
    /// Kullanıcının belirli bir rolde olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsInRoleAsync(string userId, string role);
    
    /// <summary>
    /// Kullanıcının belirli bir izne sahip olup olmadığını kontrol eder
    /// </summary>
    Task<bool> HasPermissionAsync(string userId, string permissionCode);
    
    /// <summary>
    /// Kullanıcıyı belirli bir role ekler
    /// </summary>
    Task<Result> AddToRoleAsync(string userId, string role);
    
    /// <summary>
    /// Role bir izin ekler
    /// </summary>
    Task<Result> AddPermissionToRoleAsync(string roleName, string permissionCode);
    
    /// <summary>
    /// Rolden bir izni kaldırır
    /// </summary>
    Task<Result> RemovePermissionFromRoleAsync(string roleName, string permissionCode);
}