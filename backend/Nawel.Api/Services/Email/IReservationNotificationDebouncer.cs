namespace Nawel.Api.Services.Email;

public interface IReservationNotificationDebouncer
{
    void ScheduleReservationNotification(int listOwnerId, string listOwnerName, string userName, string actionType, string giftName, string? comment = null);
}
