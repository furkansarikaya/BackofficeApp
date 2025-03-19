using Backoffice.Application.DTOs.Security;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Enums;
using Backoffice.Web.Attributes;
using Backoffice.Web.ViewModels.Security;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.Controllers;

public class IpFilterController(IIpFilterService ipFilterService) : BaseController
{
    // GET: /IpFilter
    [Permission(PermissionType.List)]
    public async Task<IActionResult> Index(string searchTerm, int pageNumber = 1)
    {
        var ipFilters = await ipFilterService.GetPagedIpFiltersAsync(pageNumber, 10, searchTerm);
        
        var viewModel = new IpFilterListViewModel
        {
            IpFilters = ipFilters,
            SearchTerm = searchTerm
        };
        
        return View(viewModel);
    }

    // GET: /IpFilter/Create
    [Permission(PermissionType.Create)]
    public IActionResult Create()
    {
        var viewModel = new IpFilterFormViewModel
        {
            IsActive = true,
            FilterType = FilterType.Allow
        };
        
        return View(viewModel);
    }

    // POST: /IpFilter/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Create)]
    public async Task<IActionResult> Create(IpFilterFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = new CreateUpdateIpFilterDto
            {
                IpAddress = viewModel.IpAddress,
                Description = viewModel.Description,
                FilterType = viewModel.FilterType,
                IsActive = viewModel.IsActive
            };
            
            var result = await ipFilterService.CreateIpFilterAsync(dto);
            
            if (result.Succeeded)
            {
                NotificationService.AddSuccessNotification("IP filtresi başarıyla oluşturuldu.");
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        
        return View(viewModel);
    }

    // GET: /IpFilter/Edit/5
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var ipFilter = await ipFilterService.GetIpFilterByIdAsync(id);
        
        if (ipFilter == null)
        {
            NotificationService.AddErrorNotification("IP filtresi bulunamadı.");
            return RedirectToAction(nameof(Index));
        }
        
        var viewModel = new IpFilterFormViewModel
        {
            Id = ipFilter.Id,
            IpAddress = ipFilter.IpAddress,
            Description = ipFilter.Description,
            FilterType = ipFilter.FilterType,
            IsActive = ipFilter.IsActive
        };
        
        return View(viewModel);
    }

    // POST: /IpFilter/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> Edit(int id, IpFilterFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            NotificationService.AddErrorNotification("Geçersiz ID.");
            return RedirectToAction(nameof(Index));
        }
        
        if (ModelState.IsValid)
        {
            var dto = new CreateUpdateIpFilterDto
            {
                Id = viewModel.Id,
                IpAddress = viewModel.IpAddress,
                Description = viewModel.Description,
                FilterType = viewModel.FilterType,
                IsActive = viewModel.IsActive
            };
            
            var result = await ipFilterService.UpdateIpFilterAsync(dto);
            
            if (result.Succeeded)
            {
                NotificationService.AddSuccessNotification("IP filtresi başarıyla güncellendi.");
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        
        return View(viewModel);
    }

    // POST: /IpFilter/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await ipFilterService.DeleteIpFilterAsync(id);
        
        if (result.Succeeded)
        {
            NotificationService.AddSuccessNotification("IP filtresi başarıyla silindi.");
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

    // POST: /IpFilter/ToggleStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permission(PermissionType.Update)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await ipFilterService.ToggleIpFilterStatusAsync(id);
        
        if (result.Succeeded)
        {
            NotificationService.AddSuccessNotification("IP filtresi durumu başarıyla değiştirildi.");
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
}