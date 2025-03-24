using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Web.ViewModels.Menu;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.ViewComponents;

public class MenuViewComponent(
    IMenuService menuService,
    IMapper mapper,
    IDbLoggerService logger)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            // Kullanıcının görme yetkisi olan menü öğelerini getir
            var menuItems = await menuService.GetUserMenuAsync();
            
            // DTO'ları view model'e dönüştür
            var viewModels = mapper.Map<List<MenuViewModel>>(menuItems);
            
            // Menü öğelerini işle (aktif öğeleri, açık/kapalı durumları belirle)
            ProcessMenuItems(viewModels);
            
            return View(viewModels);
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync("Menü öğeleri yüklenirken hata oluştu", "MenuViewComponent", ex);
            return View(new List<MenuViewModel>());
        }
    }

    /// <summary>
    /// Menü öğelerini işler - aktif menüleri belirler, açık/kapalı durumları ayarlar
    /// </summary>
    private void ProcessMenuItems(List<MenuViewModel> menuItems)
    {
        // Mevcut controller ve action'ı al
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();
        
        // Tüm menüleri varsayılan olarak kapalı yap
        CloseAllMenuItems(menuItems);
        
        // Aktif menü öğesini bul ve işaretle (bu işlem üst menüleri de açacak)
        var activeFound = MarkActiveMenuItem(menuItems, currentController, currentAction);
        
        // Aktif menü bulunamadıysa ve bu bir hata durumu ise (örneğin 404 sayfası)
        // kullanıcı deneyimini iyileştirmek için ana menüyü görünür kıl
        if (!activeFound)
        {
            // Hiçbir şey yapma - tüm menüler kapalı kalsın
        }
    }
    
    /// <summary>
    /// Tüm menü öğelerini kapalı olarak işaretler
    /// </summary>
    private static void CloseAllMenuItems(List<MenuViewModel> menuItems)
    {
        foreach (var item in menuItems)
        {
            item.IsExpanded = false;
            
            if (item.HasChildren)
            {
                CloseAllMenuItems(item.Children);
            }
        }
    }
    
    /// <summary>
    /// Aktif menü öğesini bulur ve işaretler, üst menülerini de açar
    /// </summary>
    private static bool MarkActiveMenuItem(List<MenuViewModel> menuItems, string currentController, string currentAction)
    {
        var foundActive = false;
        
        foreach (var item in menuItems)
        {
            // Aktif sayfa bu menü öğesi mi?
            var isActive = string.Equals(item.Controller, currentController, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(item.Action, currentAction, StringComparison.OrdinalIgnoreCase);
            
            if (isActive)
            {
                item.IsCurrentPage = true;
                foundActive = true;
                
                // Aktif öğenin tüm üst menülerini de aç
                ExpandParentMenus(item);
            }
            
            // Çocukları recursive olarak kontrol et
            if (!item.HasChildren) continue;
            var childIsActive = MarkActiveMenuItem(item.Children, currentController, currentAction);
                
            // Çocuklardan biri aktifse, bu öğeyi de aç
            if (!childIsActive) continue;
            item.IsExpanded = true;
            foundActive = true;
        }
        
        return foundActive;
    }
    
    /// <summary>
    /// Aktif menü öğesinin tüm üst menülerini açar
    /// </summary>
    private static void ExpandParentMenus(MenuViewModel item)
    {
        var parent = FindMenuItemParent(item);
        if (parent == null) return;
        parent.IsExpanded = true;
        ExpandParentMenus(parent);
    }
    
    /// <summary>
    /// Bir menü öğesinin ebeveynini bulur
    /// </summary>
    private static MenuViewModel FindMenuItemParent(MenuViewModel child)
    {
        return FindParentRecursive(null, child);
    }
    
    private static MenuViewModel FindParentRecursive(List<MenuViewModel> menuItems, MenuViewModel targetChild)
    {
        if (menuItems == null) return null;
        
        foreach (var item in menuItems)
        {
            if (item.Children.Contains(targetChild))
            {
                return item;
            }
            
            var found = FindParentRecursive(item.Children, targetChild);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
}