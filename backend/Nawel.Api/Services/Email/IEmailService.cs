namespace Nawel.Api.Services.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendGiftReservedNotificationsAsync(int listOwnerId, int listId, string listOwnerName, string giftName, string reservedBy, string? comment = null);
    Task SendGiftParticipationNotificationsAsync(int listOwnerId, int listId, string listOwnerName, string giftName, string participantName, string? comment = null);
    Task SendListEditedNotificationsAsync(int listOwnerId, int listId, string listOwnerName, List<string>? modifications = null);
    Task SendReservationNotificationsAsync(int listOwnerId, int listId, string listOwnerName, List<ReservationAction> actions);
    Task SendMigrationResetEmailAsync(string toEmail, string userName, string resetToken);
}
