using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>Delivers e-mails through an SMTP relay (Brevo in production) with MailKit.</summary>
public class SmtpEmailTransport(IOptions<EmailSettings> options, ILogger<SmtpEmailTransport> logger) : IEmailTransport
{
    public async Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.From.Name, settings.From.Address!));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

        var security = !settings.Smtp.EnableSsl
            ? SecureSocketOptions.None
            : settings.Smtp.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        using var client = new SmtpClient { Timeout = settings.Smtp.TimeoutSeconds * 1000 };
        await client.ConnectAsync(settings.Smtp.Host!, settings.Smtp.Port, security, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Smtp.Username))
            await client.AuthenticateAsync(settings.Smtp.Username, settings.Smtp.Password!, cancellationToken);
        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("E-mail '{Subject}' delivered to {Recipient} through SMTP.", message.Subject, message.To);
    }
}
