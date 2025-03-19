using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.DTOs.Menu;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Enums;
using Backoffice.Infrastructure.Identity;
using Backoffice.Web.Attributes;
using Backoffice.Web.ViewModels.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Backoffice.Web.Controllers;

public class MenuController(
    IMenuService menuService,
    IMapper mapper,
    IUnitOfWork unitOfWork)
    : BaseController
{
    // GET: /Menu
    [Permission(PermissionType.List)]
    public async Task<IActionResult> Index()
    {
        var menuItems = await menuService.GetMenuHierarchyAsync();
        var viewModel = new MenuListViewModel
        {
            MenuItems = mapper.Map<List<MenuViewModel>>(menuItems)
        };

        return View(viewModel);
    }

    // GET: /Menu/Create
    [Permission(PermissionType.Create)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new MenuItemFormViewModel
        {
            DisplayOrder = 100,
            IsActive = true,
            ParentMenuItems = await GetParentMenuItemsSelectList(),
            PermissionCodes = await GetPermissionCodesSelectList()
        };

        return View(viewModel);
    }

    // POST: /Menu/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Create)]
    public async Task<IActionResult> Create(MenuItemFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = mapper.Map<CreateUpdateMenuItemDto>(viewModel);
            var result = await menuService.CreateMenuItemAsync(dto);

            if (result.Succeeded)
            {
                NotificationService.AddSuccessNotification("Menü öğesi başarıyla oluşturuldu.");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // If we got this far, something failed, redisplay form
        viewModel.ParentMenuItems = await GetParentMenuItemsSelectList(viewModel.ParentId);
        viewModel.PermissionCodes = await GetPermissionCodesSelectList(viewModel.RequiredPermissionCode);

        return View(viewModel);
    }

    // GET: /Menu/Edit/5
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var menuItem = await menuService.GetMenuItemByIdAsync(id);

        if (menuItem == null)
        {
            NotificationService.AddErrorNotification("Menü öğesi bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = mapper.Map<MenuItemFormViewModel>(menuItem);
        viewModel.ParentMenuItems = await GetParentMenuItemsSelectList(viewModel.ParentId, id);
        viewModel.PermissionCodes = await GetPermissionCodesSelectList(viewModel.RequiredPermissionCode);

        return View(viewModel);
    }

    // POST: /Menu/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(int id, MenuItemFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            NotificationService.AddErrorNotification("Geçersiz menü öğesi ID.");
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            var dto = mapper.Map<CreateUpdateMenuItemDto>(viewModel);
            var result = await menuService.UpdateMenuItemAsync(dto);

            if (result.Succeeded)
            {
                NotificationService.AddSuccessNotification("Menü öğesi başarıyla güncellendi.");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // If we got this far, something failed, redisplay form
        viewModel.ParentMenuItems = await GetParentMenuItemsSelectList(viewModel.ParentId, id);
        viewModel.PermissionCodes = await GetPermissionCodesSelectList(viewModel.RequiredPermissionCode);

        return View(viewModel);
    }

    // POST: /Menu/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await menuService.DeleteMenuItemAsync(id);

        if (result.Succeeded)
        {
            NotificationService.AddSuccessNotification("Menü öğesi başarıyla silindi.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                NotificationService.AddErrorNotification(error);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Menu/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var menuItem = await menuService.GetMenuItemByIdAsync(id);

        if (menuItem == null)
        {
            NotificationService.AddErrorNotification("Menü öğesi bulunamadı.");
            return RedirectToAction(nameof(Index));
        }

        var dto = mapper.Map<CreateUpdateMenuItemDto>(menuItem);
        dto.IsActive = !dto.IsActive;

        var result = await menuService.UpdateMenuItemAsync(dto);

        if (result.Succeeded)
        {
            var status = dto.IsActive ? "aktif" : "pasif";
            NotificationService.AddSuccessNotification($"Menü öğesi {status} duruma getirildi.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                NotificationService.AddErrorNotification(error);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper methods

    private async Task<List<SelectListItem>> GetParentMenuItemsSelectList(int? selectedValue = null, int? excludeId = null)
    {
        var menuItems = await menuService.GetParentMenuItemsAsync(excludeId);

        // Create select list
        var selectList = new List<SelectListItem>
        {
            new SelectListItem { Text = "-- Üst Menü Yok --", Value = "" }
        };

        selectList.AddRange(menuItems.Select(m => new SelectListItem
        {
            Text = !string.IsNullOrEmpty(m.FullPath) ? m.FullPath : m.Name,
            Value = m.Id.ToString(),
            Selected = selectedValue.HasValue && m.Id == selectedValue.Value
        }));

        return selectList;
    }

    private async Task<List<SelectListItem>> GetPermissionCodesSelectList(string? selectedValue = null)
    {
        // Get all permissions from the database
        var permissions = await GetAllPermissions();

        // Create select list
        var selectList = new List<SelectListItem>
        {
            new() { Text = "-- İzin Gerektirmez --", Value = "" }
        };

        selectList.AddRange(permissions.Select(p => new SelectListItem
        {
            Text = p,
            Value = p,
            Selected = p == selectedValue
        }));

        return selectList;
    }

    private async Task<List<string>> GetAllPermissions()
    {
        var allPermissions = await unitOfWork.Repository<Permission, int>().GetWithIncludesAsync();
        return allPermissions.Select(p => p.Code).OrderBy(p => p).ToList();
    }
}