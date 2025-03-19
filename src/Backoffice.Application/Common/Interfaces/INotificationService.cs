using Backoffice.Application.Common.Enums;

namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Kullanıcıya bildirim mesajları göstermek için kullanılan servis arayüzü
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Belirtilen tipte bir bildirim ekler
    /// </summary>
    /// <param name="type">Bildirim tipi</param>
    /// <param name="message">Bildirim mesajı</param>
    void AddNotification(NotificationType type, string message);

    /// <summary>
    /// Başarı bildirimi ekler
    /// </summary>
    void AddSuccessNotification(string message);

    /// <summary>
    /// Hata bildirimi ekler
    /// </summary>
    void AddErrorNotification(string message);

    /// <summary>
    /// Uyarı bildirimi ekler
    /// </summary>
    void AddWarningNotification(string message);

    /// <summary>
    /// Bilgi bildirimi ekler
    /// </summary>
    void AddInfoNotification(string message);
}