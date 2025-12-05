namespace Nawel.Api.Services.Email;

public interface INotificationDebouncer
{
    void ScheduleListEditNotification(int userId, string userName, string modificationType, string? giftName = null);
}
