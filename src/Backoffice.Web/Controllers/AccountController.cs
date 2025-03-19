using System.Text.Json;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.DTOs.Auditing;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Constants;
using Backoffice.Infrastructure.Identity;
using Backoffice.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger,
    IActivityLogService activityLogService,
    ICurrentUserService currentUserService)
    : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);
                
            var user = await userManager.FindByEmailAsync(model.Email);
                
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
                
                await activityLogService.LogActivityAsync(new CreateActivityLogDto
                {
                    Category = ActivityCategories.Authentication,
                    ActivityType = ActivityTypes.Login,
                    EntityType = "ApplicationUser",
                    EntityId = user.Id
                });
            }
                
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
                
            return RedirectToAction("Index", "Home");
        }
            
        if (result.IsLockedOut)
        {
            logger.LogWarning("Kullanıcı hesabı kilitlendi: {Email}", model.Email);
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
                await activityLogService.LogActivityAsync(new CreateActivityLogDto
                {
                    Category = ActivityCategories.Authentication,
                    ActivityType = ActivityTypes.FailedLogin,
                    EntityType = "ApplicationUser",
                    EntityId = user.Id,
                    Details = JsonSerializer.Serialize(new { Reason = "Account locked out" })
                });

            return RedirectToAction(nameof(Lockout));
        }
            
        ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
        
        var failedUser = await userManager.FindByEmailAsync(model.Email);
        if (failedUser != null)
        {
            await activityLogService.LogActivityAsync(new CreateActivityLogDto
            {
                Category = ActivityCategories.Authentication,
                ActivityType = ActivityTypes.FailedLogin,
                EntityType = "ApplicationUser",
                EntityId = failedUser.Id,
                Details = JsonSerializer.Serialize(new { Reason = "Invalid credentials" })
            });
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = currentUserService.UserId;
        await signInManager.SignOutAsync();
        logger.LogInformation("Kullanıcı çıkış yaptı.");
        await activityLogService.LogActivityAsync(new CreateActivityLogDto
        {
            Category = ActivityCategories.Authentication,
            ActivityType = ActivityTypes.Logout,
            EntityType = "ApplicationUser",
            EntityId = userId
        });
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}