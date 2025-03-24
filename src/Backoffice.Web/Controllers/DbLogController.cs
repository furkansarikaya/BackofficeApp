using System.Text.Json;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Domain.Enums;
using Backoffice.Web.Attributes;
using Backoffice.Web.ViewModels.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Backoffice.Web.Controllers;

public class DbLogController(
    IDbLoggerService dbLoggerService,
    ILogger<DbLogController> logger)
    : BaseController
{
    // GET: /Log
    [Permission(PermissionType.View)]
    public async Task<IActionResult> Index(LogFilterViewModel filter, int pageNumber = 1)
    {
        try
        {
            // Log kategorilerini al
            var categories = await dbLoggerService.GetAllCategoriesAsync();
            
            // Veritabanından logları getir
            var logs = await dbLoggerService.GetLogsAsync(
                pageNumber,
                20,
                filter.Level,
                filter.Category,
                filter.SearchTerm,
                filter.FromDate,
                filter.ToDate,
                filter.UserId);
            
            // ViewModel oluştur
            var viewModel = new LogListViewModel
            {
                Logs = logs,
                Filter = filter,
                Categories = categories
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Log listesi getirilirken hata oluştu");
            NotificationService.AddErrorNotification("Log listesi getirilirken bir hata oluştu.");
            return View(new LogListViewModel());
        }
    }

    // GET: /Log/Details/5
    [Permission(PermissionType.View)]
    public async Task<IActionResult> Details(long id)
    {
        try
        {
            var logEntry = await dbLoggerService.GetLogByIdAsync(id);
            
            if (logEntry == null)
            {
                NotificationService.AddErrorNotification("Log kaydı bulunamadı.");
                return RedirectToAction(nameof(Index));
            }
            
            // Ek veriyi JSON'dan nesneye dönüştür (varsa)
            object? additionalDataObject = null;
            if (!string.IsNullOrEmpty(logEntry.AdditionalData))
            {
                try
                {
                    additionalDataObject = JsonSerializer.Deserialize<JsonElement>(logEntry.AdditionalData);
                }
                catch
                {
                    additionalDataObject = logEntry.AdditionalData;
                }
            }
            
            var viewModel = new LogDetailViewModel
            {
                LogEntry = logEntry,
                AdditionalDataObject = additionalDataObject
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Log detayı getirilirken hata oluştu");
            NotificationService.AddErrorNotification("Log detayı getirilirken bir hata oluştu.");
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Log/UserLogs/userId
    [Permission(PermissionType.View)]
    public async Task<IActionResult> UserLogs(string userId, int pageNumber = 1)
    {
        try
        {
            var filter = new LogFilterViewModel { UserId = userId };
            
            // Log kategorilerini al
            var categories = await dbLoggerService.GetAllCategoriesAsync();
            
            // Kullanıcı loglarını getir
            var logs = await dbLoggerService.GetLogsAsync(
                pageNumber,
                20,
                null,
                null,
                null,
                null,
                null,
                userId);
            
            // ViewModel oluştur
            var viewModel = new LogListViewModel
            {
                Logs = logs,
                Filter = filter,
                Categories = categories
            };
            
            ViewData["UserName"] = logs.Items.FirstOrDefault()?.UserName ?? userId;
            
            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kullanıcı logları getirilirken hata oluştu");
            NotificationService.AddErrorNotification("Kullanıcı logları getirilirken bir hata oluştu.");
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Log/CategoryLogs/CategoryName
    [Permission(PermissionType.View)]
    public async Task<IActionResult> CategoryLogs(string category, int pageNumber = 1)
    {
        try
        {
            var filter = new LogFilterViewModel { Category = category };
            
            // Log kategorilerini al
            var categories = await dbLoggerService.GetAllCategoriesAsync();
            
            // Kategori loglarını getir
            var logs = await dbLoggerService.GetLogsAsync(
                pageNumber,
                20,
                null,
                category,
                null,
                null,
                null,
                null);
            
            // ViewModel oluştur
            var viewModel = new LogListViewModel
            {
                Logs = logs,
                Filter = filter,
                Categories = categories
            };
            
            ViewData["CategoryName"] = category;
            
            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kategori logları getirilirken hata oluştu");
            NotificationService.AddErrorNotification("Kategori logları getirilirken bir hata oluştu.");
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Log/Statistics
    [Permission(PermissionType.View)]
    public async Task<IActionResult> Statistics(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            // Varsayılan tarih aralığı: son 7 gün
            var today = DateTime.Today;
            fromDate ??= today.AddDays(-7);
            toDate ??= today;
            
            // Tüm kategorileri al
            var categories = await dbLoggerService.GetAllCategoriesAsync();
            
            // Her kategorinin istatistiklerini topla
            var statisticsByCategory = new Dictionary<string, Dictionary<Domain.Enums.LogLevel, int>>();
            var totalsByLevel = new Dictionary<Domain.Enums.LogLevel, int>();
            
            // Her log seviyesi için varsayılan 0 değeri ata
            foreach (var level in Enum.GetValues<Domain.Enums.LogLevel>())
            {
                if (level != Domain.Enums.LogLevel.None)
                {
                    totalsByLevel[level] = 0;
                }
            }
            
            // Her kategori için istatistikleri hesapla
            foreach (var category in categories)
            {
                var stats = await dbLoggerService.GetLogStatisticsByCategoryAsync(category, fromDate, toDate);
                statisticsByCategory[category] = stats;
                
                // Seviye toplamlarını güncelle
                foreach (var (level, count) in stats)
                {
                    totalsByLevel[level] += count;
                }
            }
            
            // Son 24 saat ve son 7 gün için toplam log sayısını hesapla
            var last24Hours = await dbLoggerService.GetLogsAsync(
                1,
                1,
                null,
                null,
                null,
                DateTime.UtcNow.AddDays(-1),
                null,
                null);
            
            var last7Days = await dbLoggerService.GetLogsAsync(
                1,
                1,
                null,
                null,
                null,
                DateTime.UtcNow.AddDays(-7),
                null,
                null);
            
            // ViewModel oluştur
            var viewModel = new LogStatisticsViewModel
            {
                StatisticsByCategory = statisticsByCategory,
                TotalsByLevel = totalsByLevel,
                TotalLogCount = totalsByLevel.Values.Sum(),
                LogCountLast24Hours = last24Hours.TotalCount,
                LogCountLast7Days = last7Days.TotalCount,
                FromDate = fromDate.Value,
                ToDate = toDate.Value
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Log istatistikleri getirilirken hata oluştu");
            NotificationService.AddErrorNotification("Log istatistikleri getirilirken bir hata oluştu.");
            return RedirectToAction(nameof(Index));
        }
    }

    // AJAX: /Log/GetLogCountByLevel
    [HttpGet]
    public async Task<IActionResult> GetLogCountByLevel(string category = null)
    {
        try
        {
            // Son 7 gün için istatistikleri getir
            var fromDate = DateTime.Today.AddDays(-7);
            var toDate = DateTime.Today;
            
            Dictionary<Domain.Enums.LogLevel, int> stats;
            
            if (string.IsNullOrEmpty(category))
            {
                // Tüm kategoriler için toplamları hesapla
                var categories = await dbLoggerService.GetAllCategoriesAsync();
                stats = new Dictionary<Domain.Enums.LogLevel, int>();
                
                foreach (var level in Enum.GetValues<Domain.Enums.LogLevel>())
                {
                    if (level != Domain.Enums.LogLevel.None)
                    {
                        stats[level] = 0;
                    }
                }
                
                foreach (var cat in categories)
                {
                    var catStats = await dbLoggerService.GetLogStatisticsByCategoryAsync(cat, fromDate, toDate);
                    
                    foreach (var (level, count) in catStats)
                    {
                        stats[level] += count;
                    }
                }
            }
            else
            {
                // Sadece belirtilen kategori için istatistikleri getir
                stats = await dbLoggerService.GetLogStatisticsByCategoryAsync(category, fromDate, toDate);
            }
            
            // Chart.js formatına dönüştür
            var result = new
            {
                labels = stats.Keys
                    .Where(k => k != Domain.Enums.LogLevel.None)
                    .OrderBy(k => (int)k)
                    .Select(k => k.ToString())
                    .ToArray(),
                data = stats.Where(kv => kv.Key != Domain.Enums.LogLevel.None)
                    .OrderBy(kv => (int)kv.Key)
                    .Select(kv => kv.Value)
                    .ToArray()
            };
            
            return Json(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Log istatistikleri getirilirken hata oluştu");
            return Json(new { error = "İstatistikler getirilirken bir hata oluştu." });
        }
    }
}