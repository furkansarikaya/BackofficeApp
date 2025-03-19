using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;

namespace Backoffice.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager)
    : IIdentityService
{
    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.UserName;
    }
    
    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user != null && await userManager.IsInRoleAsync(user, role);
    }
    
    public async Task<bool> HasPermissionAsync(string userId, string permissionCode)
    {
        var user = await userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }
        
        // Admin rolü kontrolü - her zaman erişim izni var
        if (await userManager.IsInRoleAsync(user, "Administrator"))
        {
            return true;
        }
        
        // Kullanıcı rollerini al
        var roles = await userManager.GetRolesAsync(user);
        
        // Roller üzerinden izinleri kontrol et
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            
            if (role != null)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                
                if (claims.Any(c => c.Type == "Permission" && c.Value == permissionCode))
                {
                    return true;
                }
            }
        }
        
        // Kullanıcıya özel izinleri kontrol et
        var userClaims = await userManager.GetClaimsAsync(user);
        return userClaims.Any(c => c.Type == "Permission" && c.Value == permissionCode);
    }
    
    public async Task<Result> AddToRoleAsync(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return Result.Failure(new[] { "Kullanıcı bulunamadı." });
        }
        
        var result = await userManager.AddToRoleAsync(user, role);
        
        return result.ToApplicationResult();
    }
    
    public async Task<Result> AddPermissionToRoleAsync(string roleName, string permissionCode)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        
        if (role == null)
        {
            return Result.Failure(new[] { "Rol bulunamadı." });
        }
        
        var result = await roleManager.AddClaimAsync(role, new Claim("Permission", permissionCode));
        
        return result.ToApplicationResult();
    }
    
    public async Task<Result> RemovePermissionFromRoleAsync(string roleName, string permissionCode)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        
        if (role == null)
        {
            return Result.Failure(new[] { "Rol bulunamadı." });
        }
        
        var claims = await roleManager.GetClaimsAsync(role);
        var claim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permissionCode);
        
        if (claim == null)
        {
            return Result.Success();
        }
        
        var result = await roleManager.RemoveClaimAsync(role, claim);
        
        return result.ToApplicationResult();
    }
}