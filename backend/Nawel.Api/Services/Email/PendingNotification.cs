namespace Nawel.Api.Services.Email;

public class PendingNotification
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Modifications { get; set; } = new();
    public DateTime FirstModificationTime { get; set; }
    public DateTime LastModificationTime { get; set; }
    public System.Threading.Timer? Timer { get; set; }
}
