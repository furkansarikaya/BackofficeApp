using Backoffice.Application.Common.Enums;
using Backoffice.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Backoffice.Web.Services;

/// <summary>
/// TempData üzerinden kullanıcıya bildirim mesajları gösteren servis
/// </summary>
public class NotificationService(
    ITempDataDictionaryFactory tempDataDictionaryFactory,
    IHttpContextAccessor httpContextAccessor)
    : INotificationService
{
    /// <summary>
    /// Belirtilen tipte bir bildirim ekler
    /// </summary>
    public void AddNotification(NotificationType type, string message)
    {
        if (httpContextAccessor.HttpContext == null)
            return;

        var tempData = tempDataDictionaryFactory.GetTempData(httpContextAccessor.HttpContext);
        var key = $"Notification.{type}";

        // Eğer aynı tipte birden fazla bildirim varsa listeye ekle
        if (tempData.ContainsKey(key))
        {
            if (tempData[key] is List<string> existingMessages)
            {
                existingMessages.Add(message);
            }
            else
            {
                tempData[key] = new List<string> { message };
            }
        }
        else
        {
            tempData[key] = new List<string> { message };
        }
    }

    /// <summary>
    /// Başarı bildirimi ekler
    /// </summary>
    public void AddSuccessNotification(string message)
    {
        AddNotification(NotificationType.Success, message);
    }

    /// <summary>
    /// Hata bildirimi ekler
    /// </summary>
    public void AddErrorNotification(string message)
    {
        AddNotification(NotificationType.Error, message);
    }

    /// <summary>
    /// Uyarı bildirimi ekler
    /// </summary>
    public void AddWarningNotification(string message)
    {
        AddNotification(NotificationType.Warning, message);
    }

    /// <summary>
    /// Bilgi bildirimi ekler
    /// </summary>
    public void AddInfoNotification(string message)
    {
        AddNotification(NotificationType.Info, message);
    }
}