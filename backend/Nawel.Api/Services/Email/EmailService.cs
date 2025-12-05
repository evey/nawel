using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;

namespace Nawel.Api.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly NawelDbContext _context;
    private readonly bool _isEnabled;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, NawelDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _isEnabled = _configuration.GetValue<bool>("Email:Enabled");
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Email sending is disabled. Would have sent to {To}: {Subject}", to, subject);
            return;
        }

        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"];
            var useSsl = _configuration.GetValue<bool>("Email:UseSsl", true);

            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail, fromName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = useSsl;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // Ne pas lancer d'exception pour ne pas bloquer l'application
        }
    }

    public async Task SendGiftReservedNotificationsAsync(int listOwnerId, string listOwnerName, string giftName, string reservedBy, string? comment = null)
    {
        // Récupérer tous les utilisateurs qui ont activé la notification NotifyGiftTaken, sauf le propriétaire de la liste
        var usersToNotify = await _context.Users
            .Where(u => u.NotifyGiftTaken && u.Id != listOwnerId && !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync();

        var subject = $"🎁 Cadeau réservé sur la liste de {listOwnerName} : {giftName}";
        var commentSection = !string.IsNullOrEmpty(comment)
            ? $@"
                <div style=""background-color: #f0f7ff; padding: 15px; border-radius: 5px; margin-top: 15px;"">
                    <strong>Commentaire :</strong>
                    <p style=""margin: 5px 0 0 0; white-space: pre-wrap;"">{comment}</p>
                </div>"
            : "";

        foreach (var user in usersToNotify)
        {
            var userName = user.FirstName ?? user.Login;
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <div style=""background: linear-gradient(135deg, #1b5e20 0%, #2e7d32 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
            <h1 style=""margin: 0; font-size: 28px;"">🎄 Nawel - Listes de Noël 🎄</h1>
        </div>

        <div style=""background-color: #ffffff; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 10px 10px;"">
            <h2 style=""color: #1b5e20; margin-top: 0;"">Bonjour {userName},</h2>

            <p style=""font-size: 16px;"">
                <strong>{reservedBy}</strong> vient de réserver un cadeau de la liste de <strong>{listOwnerName}</strong> !
            </p>

            <div style=""background-color: #fff3e0; padding: 20px; border-left: 4px solid #ff9800; margin: 20px 0;"">
                <h3 style=""margin: 0 0 10px 0; color: #e65100;"">🎁 {giftName}</h3>
            </div>

            {commentSection}

            <p style=""margin-top: 30px; font-size: 14px; color: #666;"">
                Vous recevez cet email car vous avez activé les notifications pour les réservations de cadeaux.
            </p>
        </div>

        <div style=""text-align: center; padding: 20px; color: #666; font-size: 12px;"">
            <p>Nawel - Votre liste de cadeaux de Noël 🎅</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email!, subject, htmlBody);
        }
    }

    public async Task SendGiftParticipationNotificationsAsync(int listOwnerId, string listOwnerName, string giftName, string participantName, string? comment = null)
    {
        // Récupérer tous les utilisateurs qui ont activé la notification NotifyGiftTaken, sauf le propriétaire de la liste
        var usersToNotify = await _context.Users
            .Where(u => u.NotifyGiftTaken && u.Id != listOwnerId && !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync();

        var subject = $"🎁 Participation à un cadeau groupé de {listOwnerName} : {giftName}";
        var commentSection = !string.IsNullOrEmpty(comment)
            ? $@"
                <div style=""background-color: #f0f7ff; padding: 15px; border-radius: 5px; margin-top: 15px;"">
                    <strong>Commentaire :</strong>
                    <p style=""margin: 5px 0 0 0; white-space: pre-wrap;"">{comment}</p>
                </div>"
            : "";

        foreach (var user in usersToNotify)
        {
            var userName = user.FirstName ?? user.Login;
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <div style=""background: linear-gradient(135deg, #1b5e20 0%, #2e7d32 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
            <h1 style=""margin: 0; font-size: 28px;"">🎄 Nawel - Listes de Noël 🎄</h1>
        </div>

        <div style=""background-color: #ffffff; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 10px 10px;"">
            <h2 style=""color: #1b5e20; margin-top: 0;"">Bonjour {userName},</h2>

            <p style=""font-size: 16px;"">
                <strong>{participantName}</strong> vient de participer à un cadeau groupé de la liste de <strong>{listOwnerName}</strong> !
            </p>

            <div style=""background-color: #e8f5e9; padding: 20px; border-left: 4px solid #4caf50; margin: 20px 0;"">
                <h3 style=""margin: 0 0 10px 0; color: #2e7d32;"">🎁 {giftName}</h3>
                <p style=""margin: 0; color: #1b5e20;"">👥 Cadeau groupé</p>
            </div>

            {commentSection}

            <p style=""margin-top: 30px; font-size: 14px; color: #666;"">
                Vous recevez cet email car vous avez activé les notifications pour les réservations de cadeaux.
            </p>
        </div>

        <div style=""text-align: center; padding: 20px; color: #666; font-size: 12px;"">
            <p>Nawel - Votre liste de cadeaux de Noël 🎅</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email!, subject, htmlBody);
        }
    }

    public async Task SendListEditedNotificationsAsync(int listOwnerId, string listOwnerName, List<string>? modifications = null)
    {
        // Récupérer tous les utilisateurs qui ont activé la notification NotifyListEdit, sauf le propriétaire de la liste
        var usersToNotify = await _context.Users
            .Where(u => u.NotifyListEdit && u.Id != listOwnerId && !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync();

        var subject = $"📝 {listOwnerName} a modifié sa liste de cadeaux";

        // Construire la section des modifications si elles sont fournies
        var modificationsSection = "";
        if (modifications != null && modifications.Any())
        {
            var modificationsList = string.Join("", modifications.Select(m =>
                $"<li style=\"margin: 5px 0;\">{m}</li>"));

            modificationsSection = $@"
                <div style=""background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                    <h3 style=""margin: 0 0 10px 0; color: #1b5e20; font-size: 16px;"">📋 Modifications :</h3>
                    <ul style=""margin: 0; padding-left: 20px;"">
                        {modificationsList}
                    </ul>
                </div>";
        }

        foreach (var user in usersToNotify)
        {
            var userName = user.FirstName ?? user.Login;
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <div style=""background: linear-gradient(135deg, #1b5e20 0%, #2e7d32 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
            <h1 style=""margin: 0; font-size: 28px;"">🎄 Nawel - Listes de Noël 🎄</h1>
        </div>

        <div style=""background-color: #ffffff; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 10px 10px;"">
            <h2 style=""color: #1b5e20; margin-top: 0;"">Bonjour {userName},</h2>

            <p style=""font-size: 16px;"">
                <strong>{listOwnerName}</strong> vient de modifier sa liste de cadeaux de Noël.
            </p>

            {modificationsSection}

            <p style=""font-size: 14px; color: #666;"">
                {(modifications != null && modifications.Any()
                    ? "Ces changements ont été apportés récemment."
                    : "Des changements ont été apportés à sa liste.")} N'hésitez pas à aller voir les nouveautés !
            </p>

            <p style=""margin-top: 30px; font-size: 14px; color: #666;"">
                Vous recevez cet email car vous avez activé les notifications pour les modifications de listes.
            </p>
        </div>

        <div style=""text-align: center; padding: 20px; color: #666; font-size: 12px;"">
            <p>Nawel - Votre liste de cadeaux de Noël 🎅</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email!, subject, htmlBody);
        }
    }

    public async Task SendReservationNotificationsAsync(int listOwnerId, string listOwnerName, List<ReservationAction> actions)
    {
        // Récupérer tous les utilisateurs qui ont activé la notification NotifyGiftTaken, sauf le propriétaire de la liste
        var usersToNotify = await _context.Users
            .Where(u => u.NotifyGiftTaken && u.Id != listOwnerId && !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync();

        var subject = $"🎁 Activité sur la liste de {listOwnerName}";

        // Grouper les actions par utilisateur
        var actionsByUser = actions
            .GroupBy(a => a.UserName)
            .OrderBy(g => g.First().Timestamp);

        // Construire la section des actions
        var actionsSection = "";
        foreach (var userActions in actionsByUser)
        {
            var userName = userActions.Key;
            var actionsList = string.Join("", userActions.Select(a =>
            {
                var actionText = a.ActionType switch
                {
                    "reserve" => $"Réservé : {a.GiftName}",
                    "unreserve" => $"Annulé : {a.GiftName}",
                    "participate" => $"Participé au cadeau groupé : {a.GiftName}",
                    "unparticipate" => $"Annulé sa participation : {a.GiftName}",
                    _ => $"Action : {a.GiftName}"
                };

                var commentHtml = !string.IsNullOrEmpty(a.Comment)
                    ? $"<span style=\"color: #666; font-size: 13px;\"> - {a.Comment}</span>"
                    : "";

                return $"<li style=\"margin: 5px 0;\">{actionText}{commentHtml}</li>";
            }));

            actionsSection += $@"
                <div style=""margin-bottom: 15px;"">
                    <strong style=""color: #1b5e20;"">{userName} :</strong>
                    <ul style=""margin: 5px 0; padding-left: 20px;"">
                        {actionsList}
                    </ul>
                </div>";
        }

        // Calculer les statistiques
        var reserveCount = actions.Count(a => a.ActionType == "reserve" || a.ActionType == "participate");
        var unreserveCount = actions.Count(a => a.ActionType == "unreserve" || a.ActionType == "unparticipate");
        var statsText = reserveCount > 0 || unreserveCount > 0
            ? $"({reserveCount} réservation{(reserveCount > 1 ? "s" : "")}" +
              (unreserveCount > 0 ? $", {unreserveCount} annulation{(unreserveCount > 1 ? "s" : "")}" : "") +
              " dans les dernières minutes)"
            : "";

        foreach (var user in usersToNotify)
        {
            var userName = user.FirstName ?? user.Login;
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <div style=""background: linear-gradient(135deg, #1b5e20 0%, #2e7d32 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
            <h1 style=""margin: 0; font-size: 28px;"">🎄 Nawel - Listes de Noël 🎄</h1>
        </div>

        <div style=""background-color: #ffffff; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 10px 10px;"">
            <h2 style=""color: #1b5e20; margin-top: 0;"">Bonjour {userName},</h2>

            <p style=""font-size: 16px;"">
                Des actions ont été effectuées sur la liste de <strong>{listOwnerName}</strong> :
            </p>

            <div style=""background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                {actionsSection}
            </div>

            <p style=""font-size: 13px; color: #999; text-align: center;"">
                {statsText}
            </p>

            <p style=""margin-top: 30px; font-size: 14px; color: #666;"">
                Vous recevez cet email car vous avez activé les notifications pour les réservations de cadeaux.
            </p>
        </div>

        <div style=""text-align: center; padding: 20px; color: #666; font-size: 12px;"">
            <p>Nawel - Votre liste de cadeaux de Noël 🎅</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email!, subject, htmlBody);
        }
    }
}
