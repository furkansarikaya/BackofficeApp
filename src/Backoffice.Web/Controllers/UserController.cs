using Backoffice.Application.Common.Models;
using Backoffice.Domain.Constants;
using Backoffice.Domain.Enums;
using Backoffice.Infrastructure.Identity;
using Backoffice.Web.Attributes;
using Backoffice.Web.Filters;
using Backoffice.Web.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Backoffice.Web.Controllers;

public class UserController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager)
    : BaseController
{
    // GET: /User
    [Permission(PermissionType.List)]
    public async Task<IActionResult> Index(string searchTerm, int pageNumber = 1, bool showInactive = false)
    {
        // Kullanıcıları sorgula
        var query = userManager.Users.AsQueryable();

        // İnaktif kullanıcıları filtrele
        if (!showInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        // Arama filtrelemesi
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => 
                u.UserName.ToLower().Contains(searchTerm) || 
                u.Email.ToLower().Contains(searchTerm) || 
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)));
        }

        // Toplam kayıt sayısı
        var totalCount = await query.CountAsync();

        // Sıralama ve sayfalama
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * 10)
            .Take(10)
            .ToListAsync();

        // Kullanıcı listesi ViewModel
        var userViewModels = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userViewModels.Add(new UserListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            });
        }

        // Sayfalı liste oluştur
        var paginatedList = new PaginatedList<UserListItemViewModel>(
            userViewModels, 
            totalCount, 
            pageNumber, 
            10);

        var viewModel = new UserListViewModel
        {
            Users = paginatedList,
            SearchTerm = searchTerm,
            ShowInactive = showInactive
        };

        return View(viewModel);
    }

    // GET: /User/Details/5
    [Permission(PermissionType.View)]
    public async Task<IActionResult> Details(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var roles = await userManager.GetRolesAsync(user);
        
        var viewModel = new UserDetailViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList()
        };

        return View(viewModel);
    }

    // GET: /User/Create
    [Permission(PermissionType.Create)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new UserCreateViewModel
        {
            AvailableRoles = await GetRolesAsSelectListItems()
        };
        
        return View(viewModel);
    }

    // POST: /User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Create)]
    [LogActivity(ActivityCategories.UserManagement, ActivityTypes.Create, true)]
    public async Task<IActionResult> Create(UserCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                IsActive = viewModel.IsActive,
                EmailConfirmed = true // Backoffice kullanıcıları için otomatik onay
            };

            var result = await userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                // Rolleri ekle
                if (viewModel.SelectedRoles != null && viewModel.SelectedRoles.Any())
                {
                    await userManager.AddToRolesAsync(user, viewModel.SelectedRoles);
                }

                NotificationService.AddSuccessNotification("Kullanıcı başarıyla oluşturuldu.");
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        // Hata durumunda rolleri yeniden yükle
        viewModel.AvailableRoles = await GetRolesAsSelectListItems(viewModel.SelectedRoles);
        return View(viewModel);
    }

    // GET: /User/Edit/5
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var userRoles = await userManager.GetRolesAsync(user);
        
        var viewModel = new UserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            SelectedRoles = userRoles.ToList(),
            AvailableRoles = await GetRolesAsSelectListItems(userRoles)
        };

        return View(viewModel);
    }

    // POST: /User/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.UserManagement, ActivityTypes.Update, true)]
    public async Task<IActionResult> Edit(string id, UserEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            NotificationService.AddErrorNotification("Geçersiz kullanıcı ID.");
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
                return RedirectToAction(nameof(Index));
            }

            // Admin kullanıcısını pasif yapma kontrolü
            if (await userManager.IsInRoleAsync(user, "Administrator") && !viewModel.IsActive)
            {
                NotificationService.AddErrorNotification("Administrator rolündeki kullanıcılar pasif yapılamaz.");
                viewModel.IsActive = true;
                viewModel.AvailableRoles = await GetRolesAsSelectListItems(viewModel.SelectedRoles);
                return View(viewModel);
            }

            user.UserName = viewModel.UserName;
            user.Email = viewModel.Email;
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            user.IsActive = viewModel.IsActive;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Mevcut rolleri al
                var currentRoles = await userManager.GetRolesAsync(user);
                
                // Kaldırılacak roller
                var rolesToRemove = currentRoles.Where(r => !viewModel.SelectedRoles.Contains(r)).ToList();
                if (rolesToRemove.Any())
                {
                    await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
                
                // Eklenecek roller
                var rolesToAdd = viewModel.SelectedRoles.Where(r => !currentRoles.Contains(r)).ToList();
                if (rolesToAdd.Any())
                {
                    await userManager.AddToRolesAsync(user, rolesToAdd);
                }

                NotificationService.AddSuccessNotification("Kullanıcı başarıyla güncellendi.");
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        // Hata durumunda rolleri yeniden yükle
        viewModel.AvailableRoles = await GetRolesAsSelectListItems(viewModel.SelectedRoles);
        return View(viewModel);
    }

    // GET: /User/ChangePassword/5
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> ChangePassword(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ChangePasswordViewModel
        {
            UserId = user.Id,
            UserName = user.UserName
        };

        return View(viewModel);
    }

    // POST: /User/ChangePassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.UserManagement, ActivityTypes.Update, true)]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByIdAsync(viewModel.UserId);
            
            if (user == null)
            {
                NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
                return RedirectToAction(nameof(Index));
            }

            // Şifreyi sıfırla
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, viewModel.NewPassword);

            if (result.Succeeded)
            {
                NotificationService.AddSuccessNotification("Kullanıcı şifresi başarıyla değiştirildi.");
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        return View(viewModel);
    }

    // POST: /User/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    [LogActivity(ActivityCategories.UserManagement, ActivityTypes.Update, true)]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        // Administrator rolündeki kullanıcıyı pasif yapma kontrolü
        if (await userManager.IsInRoleAsync(user, "Administrator") && user.IsActive)
        {
            NotificationService.AddErrorNotification("Administrator rolündeki kullanıcılar pasif yapılamaz.");
            return RedirectToAction(nameof(Details), new { id });
        }

        user.IsActive = !user.IsActive;
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            NotificationService.AddSuccessNotification($"Kullanıcı {(user.IsActive ? "aktif" : "pasif")} duruma getirildi.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                NotificationService.AddErrorNotification(error.Description);
            }
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /User/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Delete)]
    [LogActivity(ActivityCategories.UserManagement, ActivityTypes.Delete, true)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            NotificationService.AddErrorNotification("Kullanıcı bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        // Administrator rolündeki kullanıcıyı silme kontrolü
        if (await userManager.IsInRoleAsync(user, "Administrator"))
        {
            NotificationService.AddErrorNotification("Administrator rolündeki kullanıcılar silinemez.");
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            NotificationService.AddSuccessNotification("Kullanıcı başarıyla silindi.");
            return RedirectToAction(nameof(Index));
        }
        
        foreach (var error in result.Errors)
        {
            NotificationService.AddErrorNotification(error.Description);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    #region Helper Methods

    private async Task<List<SelectListItem>> GetRolesAsSelectListItems(IEnumerable<string>? selectedRoles = null)
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        var selectedRolesList = selectedRoles?.ToList() ?? new List<string>();
        
        return roles.Select(r => new SelectListItem
        {
            Text = $"{r.Name} {(!string.IsNullOrEmpty(r.Description) ? $" - {r.Description}" : "")}",
            Value = r.Name,
            Selected = selectedRolesList.Contains(r.Name)
        }).ToList();
    }

    #endregion
}