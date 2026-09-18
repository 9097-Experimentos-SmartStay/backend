using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>
///     Development transport (<c>Email:Transport=Log</c>, the default outside Production): writes the whole e-mail
///     (plain-text body, including its links) to the log so flows such as e-mail verification or password reset can
///     be completed locally. Never used in Production (the settings validator requires a real transport there),
///     because the body carries account tokens.
/// </summary>
public class LoggingEmailTransport(ILogger<LoggingEmailTransport> logger) : IEmailTransport
{
    public Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Email:Transport is Log: e-mail written to the log instead of being sent.\nTo: {Recipient}\nSubject: {Subject}\n{Body}",
            message.To, message.Subject, message.TextBody);
        return Task.CompletedTask;
    }
}
