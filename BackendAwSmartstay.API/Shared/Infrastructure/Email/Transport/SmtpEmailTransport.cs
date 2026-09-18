using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>
///     Delivers e-mails through an SMTP relay with MailKit (<c>Email:Transport=Smtp</c>), e.g. a local test server.
///     Production uses the Brevo HTTP API instead: outbound SMTP ports are unreliable on the hosting provider.
/// </summary>
public class SmtpEmailTransport(IOptions<EmailSettings> options) : IEmailTransport
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
        try
        {
            await client.ConnectAsync(settings.Smtp.Host!, settings.Smtp.Port, security, cancellationToken);
            if (!string.IsNullOrWhiteSpace(settings.Smtp.Username))
                await client.AuthenticateAsync(settings.Smtp.Username, settings.Smtp.Password!, cancellationToken);
            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (AuthenticationException exception)
        {
            throw new EmailDeliveryException("SMTP authentication failed.", isPermanent: true, exception);
        }
        catch (SmtpCommandException exception) when ((int)exception.StatusCode >= 500)
        {
            // 5xx replies are permanent (RFC 5321): unknown recipient, sender not allowed...
            throw new EmailDeliveryException($"SMTP {(int)exception.StatusCode} {exception.ErrorCode}.", isPermanent: true, exception);
        }
    }
}
