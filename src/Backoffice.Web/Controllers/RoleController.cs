using System.Security.Claims;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Constants;
using Backoffice.Domain.Enums;
using Backoffice.Infrastructure.Identity;
using Backoffice.Web.Attributes;
using Backoffice.Web.Filters;
using Backoffice.Web.ViewModels.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backoffice.Web.Controllers;

public class RoleController(
    IUnitOfWork unitOfWork,
    RoleManager<ApplicationRole> roleManager,
    ILogger<RoleController> logger)
    : BaseController
{
    // GET: /Role
    [Permission(PermissionType.List)]
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles.ToListAsync();
        
        var viewModel = new RoleListViewModel
        {
            Roles = roles
        };
        
        return View(viewModel);
    }

    // GET: /Role/Create
    [Permission(PermissionType.Create)]
    public IActionResult Create()
    {
        return View(new RoleFormViewModel());
    }

    // POST: /Role/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Create)]
    [LogActivity(ActivityCategories.RoleManagement, ActivityTypes.Create, true)]
    public async Task<IActionResult> Create(RoleFormViewModel viewModel)
    {
        if (!ModelState.IsValid) return View(viewModel);
        var role = new ApplicationRole
        {
            Name = viewModel.Name,
            Description = viewModel.Description
        };

        var result = await roleManager.CreateAsync(role);
            
        if (result.Succeeded)
        {
            logger.LogInformation("Rol oluşturuldu: {Name}", viewModel.Name);
            NotificationService.AddSuccessNotification("Rol başarıyla oluşturuldu.");
            return RedirectToAction(nameof(Index));
        }
            
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(viewModel);
    }

    // GET: /Role/Edit/5
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        
        if (role == null)
        {
            return NotFound();
        }
        
        var viewModel = new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description
        };
        
        return View(viewModel);
    }

    // POST: /Role/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.RoleManagement, ActivityTypes.Update, true)]
    public async Task<IActionResult> Edit(string id, RoleFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid) return View(viewModel);
        var role = await roleManager.FindByIdAsync(viewModel.Id);
            
        if (role == null)
        {
            return NotFound();
        }

        // Administrator rolünün adını değiştirmeye izin verme
        if (role.Name == "Administrator" && role.Name != viewModel.Name)
        {
            ModelState.AddModelError(string.Empty, "Administrator rolünün adı değiştirilemez.");
            return View(viewModel);
        }

        role.Name = viewModel.Name;
        role.Description = viewModel.Description;

        var result = await roleManager.UpdateAsync(role);
            
        if (result.Succeeded)
        {
            logger.LogInformation("Rol güncellendi: {Name}", viewModel.Name);
            
            NotificationService.AddSuccessNotification("Rol başarıyla güncellendi.");
            return RedirectToAction(nameof(Index));
        }
            
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(viewModel);
    }

    // GET: /Role/Permissions/5
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Permissions(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        
        if (role == null)
        {
            return NotFound();
        }

        // Tüm izinleri al
        var allPermissions = await unitOfWork.Repository<Permission, int>().GetWithIncludesAsync();
        
        // Controller gruplarına göre sırala
        var groupedPermissions = allPermissions
            .GroupBy(p => p.Controller)
            .ToDictionary(g => g.Key, g => g.ToList());
            
        // Rol izinlerini al
        var roleClaims = await roleManager.GetClaimsAsync(role);
        var rolePermissionCodes = roleClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();
            
        // ViewModel oluştur
        var viewModel = new RolePermissionsViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            Description = role.Description,
            ControllerGroups = groupedPermissions.Select(g => new ControllerPermissionsViewModel
            {
                ControllerName = g.Key,
                Permissions = g.Value.Select(p => new PermissionViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    IsSelected = rolePermissionCodes.Contains(p.Code)
                }).ToList()
            }).ToList()
        };
        
        return View(viewModel);
    }

    // POST: /Role/Permissions/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.RoleManagement, ActivityTypes.Update, true)]
    public async Task<IActionResult> Permissions(RolePermissionsViewModel viewModel)
    {
        var role = await roleManager.FindByIdAsync(viewModel.RoleId);
        
        if (role == null)
        {
            return NotFound();
        }
        
        // Administrator rolünün izinlerini değiştirmeye izin verme
        if (role.Name == "Administrator")
        {
            NotificationService.AddErrorNotification("Administrator rolünün izinleri değiştirilemez.");
            return RedirectToAction(nameof(Index));
        }

        // Mevcut izinleri al
        var existingClaims = await roleManager.GetClaimsAsync(role);
        var permissionClaims = existingClaims.Where(c => c.Type == "Permission").ToList();
        
        // Seçilen izin kodlarını topla
        var selectedPermissionCodes = new List<string>();
        
        foreach (var group in viewModel.ControllerGroups)
        {
            foreach (var permission in group.Permissions)
            {
                if (permission.IsSelected)
                {
                    selectedPermissionCodes.Add(permission.Code);
                }
            }
        }

        // Kaldırılacak izinler
        foreach (var claim in permissionClaims)
        {
            if (!selectedPermissionCodes.Contains(claim.Value))
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }
        }

        // Eklenecek izinler
        var existingPermissionCodes = permissionClaims.Select(c => c.Value).ToList();
        
        foreach (var code in selectedPermissionCodes)
        {
            if (!existingPermissionCodes.Contains(code))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", code));
            }
        }
        
        logger.LogInformation("Rol izinleri güncellendi: {Name}", role.Name);
        
        NotificationService.AddSuccessNotification("Rol izinleri başarıyla güncellendi.");
        return RedirectToAction(nameof(Index));
    }

    // POST: /Role/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Delete)]
    [LogActivity(ActivityCategories.RoleManagement, ActivityTypes.Delete, true)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        
        if (role == null)
        {
            return NotFound();
        }

        // Administrator rolünü silmeye izin verme
        if (role.Name == "Administrator")
        {
            NotificationService.AddErrorNotification("Administrator rolü silinemez.");
            return RedirectToAction(nameof(Index));
        }

        var result = await roleManager.DeleteAsync(role);
        
        if (result.Succeeded)
        {
            logger.LogInformation("Rol silindi: {Name}", role.Name);
            
            NotificationService.AddSuccessNotification("Rol başarıyla silindi.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                NotificationService.AddErrorNotification(error.Description);
            }
        }
        
        return RedirectToAction(nameof(Index));
    }
}