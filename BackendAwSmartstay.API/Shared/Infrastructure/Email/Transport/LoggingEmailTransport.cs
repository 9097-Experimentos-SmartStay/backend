using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>
///     Development transport used when no SMTP relay is configured: writes the whole e-mail (plain-text body,
///     including its links) to the log so flows such as e-mail verification or password reset can be completed
///     locally. Never registered in Production (the settings validator requires SMTP there).
/// </summary>
public class LoggingEmailTransport(ILogger<LoggingEmailTransport> logger) : IEmailTransport
{
    public Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "SMTP is not configured: e-mail written to the log instead of being sent.\nTo: {Recipient}\nSubject: {Subject}\n{Body}",
            message.To, message.Subject, message.TextBody);
        return Task.CompletedTask;
    }
}
