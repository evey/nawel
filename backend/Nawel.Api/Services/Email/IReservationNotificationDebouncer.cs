namespace Nawel.Api.Services.Email;

public interface IReservationNotificationDebouncer
{
    void ScheduleReservationNotification(int listOwnerId, int listId, string listOwnerName, string userName, string actionType, string giftName, string? comment = null);
}
