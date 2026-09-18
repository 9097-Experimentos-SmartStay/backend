using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>Actually hands a message to a mail system (SMTP relay, or the log in development).</summary>
public interface IEmailTransport
{
    Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken);
}
