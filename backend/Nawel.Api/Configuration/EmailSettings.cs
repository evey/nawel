namespace Nawel.Api.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = false;
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "no-reply@nawel.com";
    public string FromName { get; set; } = "Nawel - Listes de Noël";
    public bool UseSsl { get; set; } = false;
    public int NotificationDelayMinutes { get; set; } = 2;
    public int ReservationNotificationDelayMinutes { get; set; } = 2;

    public void Validate()
    {
        if (!Enabled)
        {
            return; // No validation needed if email is disabled
        }

        if (string.IsNullOrWhiteSpace(SmtpHost))
        {
            throw new InvalidOperationException("Email SMTP host must be configured when email is enabled");
        }

        if (SmtpPort <= 0 || SmtpPort > 65535)
        {
            throw new InvalidOperationException("Email SMTP port must be between 1 and 65535");
        }

        if (string.IsNullOrWhiteSpace(FromEmail))
        {
            throw new InvalidOperationException("Email FromEmail must be configured when email is enabled");
        }
    }
}
