namespace Nawel.Api.Services.Email;

public class ReservationAction
{
    public string UserName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty; // "reserve", "unreserve", "participate", "unparticipate"
    public string GiftName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; }
}
