using System.Collections.Concurrent;

namespace Nawel.Api.Services.Email;

public class NotificationDebouncer : INotificationDebouncer, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDebouncer> _logger;
    private readonly ConcurrentDictionary<int, PendingNotification> _pendingNotifications = new();
    private readonly int _delayMinutes;

    public NotificationDebouncer(IServiceProvider serviceProvider, ILogger<NotificationDebouncer> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _delayMinutes = configuration.GetValue<int>("Email:NotificationDelayMinutes", 15);
    }

    public void ScheduleListEditNotification(int userId, string userName, string modificationType, string? giftName = null)
    {
        var modificationText = modificationType switch
        {
            "add" => $"Ajouté : {giftName}",
            "update" => $"Modifié : {giftName}",
            "delete" => $"Supprimé : {giftName}",
            _ => "Modification"
        };

        _pendingNotifications.AddOrUpdate(
            userId,
            // Si pas de notification en attente, on en crée une nouvelle
            key =>
            {
                var notification = new PendingNotification
                {
                    UserId = userId,
                    UserName = userName,
                    FirstModificationTime = DateTime.UtcNow,
                    LastModificationTime = DateTime.UtcNow,
                    Modifications = new List<string> { modificationText }
                };

                // Créer le timer qui se déclenchera après le délai
                notification.Timer = new System.Threading.Timer(
                    callback: _ => SendNotification(userId),
                    state: null,
                    dueTime: TimeSpan.FromMinutes(_delayMinutes),
                    period: Timeout.InfiniteTimeSpan
                );

                _logger.LogInformation("Scheduled notification for user {UserId} ({UserName}) - will send in {Delay} minutes",
                    userId, userName, _delayMinutes);

                return notification;
            },
            // Si une notification existe déjà, on la met à jour
            (key, existingNotification) =>
            {
                existingNotification.Modifications.Add(modificationText);
                existingNotification.LastModificationTime = DateTime.UtcNow;

                // Annuler l'ancien timer et en créer un nouveau
                existingNotification.Timer?.Dispose();
                existingNotification.Timer = new System.Threading.Timer(
                    callback: _ => SendNotification(userId),
                    state: null,
                    dueTime: TimeSpan.FromMinutes(_delayMinutes),
                    period: Timeout.InfiniteTimeSpan
                );

                _logger.LogInformation("Reset notification timer for user {UserId} ({UserName}) - {Count} modifications pending",
                    userId, userName, existingNotification.Modifications.Count);

                return existingNotification;
            }
        );
    }

    private async void SendNotification(int userId)
    {
        if (!_pendingNotifications.TryRemove(userId, out var notification))
        {
            return;
        }

        try
        {
            notification.Timer?.Dispose();

            _logger.LogInformation("Sending aggregated notification for user {UserId} ({UserName}) with {Count} modifications",
                userId, notification.UserName, notification.Modifications.Count);

            // Créer un scope pour résoudre les services scoped
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Envoyer l'email avec le résumé des modifications
            await emailService.SendListEditedNotificationsAsync(
                userId,
                notification.UserName,
                notification.Modifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending aggregated notification for user {UserId}", userId);
        }
    }

    public void Dispose()
    {
        foreach (var notification in _pendingNotifications.Values)
        {
            notification.Timer?.Dispose();
        }
        _pendingNotifications.Clear();
    }
}
