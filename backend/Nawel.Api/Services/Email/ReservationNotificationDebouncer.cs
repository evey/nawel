using System.Collections.Concurrent;

namespace Nawel.Api.Services.Email;

public class ReservationNotificationDebouncer : IReservationNotificationDebouncer, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationNotificationDebouncer> _logger;
    private readonly ConcurrentDictionary<int, PendingReservationNotification> _pendingNotifications = new();
    private readonly int _delayMinutes;

    public ReservationNotificationDebouncer(IServiceProvider serviceProvider, ILogger<ReservationNotificationDebouncer> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _delayMinutes = configuration.GetValue<int>("Email:ReservationNotificationDelayMinutes", 5);
    }

    public void ScheduleReservationNotification(int listOwnerId, string listOwnerName, string userName, string actionType, string giftName, string? comment = null)
    {
        var action = new ReservationAction
        {
            UserName = userName,
            ActionType = actionType,
            GiftName = giftName,
            Comment = comment,
            Timestamp = DateTime.UtcNow
        };

        _pendingNotifications.AddOrUpdate(
            listOwnerId,
            // Si pas de notification en attente pour cette liste, on en crée une nouvelle
            key =>
            {
                var notification = new PendingReservationNotification
                {
                    ListOwnerId = listOwnerId,
                    ListOwnerName = listOwnerName,
                    FirstActionTime = DateTime.UtcNow,
                    LastActionTime = DateTime.UtcNow,
                    Actions = new List<ReservationAction> { action }
                };

                // Créer le timer qui se déclenchera après le délai
                notification.Timer = new System.Threading.Timer(
                    callback: _ => SendNotification(listOwnerId),
                    state: null,
                    dueTime: TimeSpan.FromMinutes(_delayMinutes),
                    period: Timeout.InfiniteTimeSpan
                );

                _logger.LogInformation("Scheduled reservation notification for list owner {OwnerId} ({OwnerName}) - will send in {Delay} minutes",
                    listOwnerId, listOwnerName, _delayMinutes);

                return notification;
            },
            // Si une notification existe déjà, on la met à jour
            (key, existingNotification) =>
            {
                existingNotification.Actions.Add(action);
                existingNotification.LastActionTime = DateTime.UtcNow;

                // Annuler l'ancien timer et en créer un nouveau
                existingNotification.Timer?.Dispose();
                existingNotification.Timer = new System.Threading.Timer(
                    callback: _ => SendNotification(listOwnerId),
                    state: null,
                    dueTime: TimeSpan.FromMinutes(_delayMinutes),
                    period: Timeout.InfiniteTimeSpan
                );

                _logger.LogInformation("Reset reservation notification timer for list owner {OwnerId} ({OwnerName}) - {Count} actions pending",
                    listOwnerId, listOwnerName, existingNotification.Actions.Count);

                return existingNotification;
            }
        );
    }

    private async void SendNotification(int listOwnerId)
    {
        if (!_pendingNotifications.TryRemove(listOwnerId, out var notification))
        {
            return;
        }

        try
        {
            notification.Timer?.Dispose();

            _logger.LogInformation("Sending aggregated reservation notification for list owner {OwnerId} ({OwnerName}) with {Count} actions",
                listOwnerId, notification.ListOwnerName, notification.Actions.Count);

            // Créer un scope pour résoudre les services scoped
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Envoyer l'email avec le résumé des actions
            await emailService.SendReservationNotificationsAsync(
                listOwnerId,
                notification.ListOwnerName,
                notification.Actions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending aggregated reservation notification for list owner {OwnerId}", listOwnerId);
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
