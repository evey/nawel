namespace Nawel.Api.Services.Email;

public class PendingReservationNotification
{
    public int ListOwnerId { get; set; }
    public string ListOwnerName { get; set; } = string.Empty;
    public List<ReservationAction> Actions { get; set; } = new();
    public DateTime FirstActionTime { get; set; }
    public DateTime LastActionTime { get; set; }
    public System.Threading.Timer? Timer { get; set; }
}
