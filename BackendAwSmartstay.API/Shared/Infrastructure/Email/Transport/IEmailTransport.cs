using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>Actually hands a message to a mail system (Brevo API, an SMTP relay, or the log in development).</summary>
/// <remarks>
///     Returning means the mail system accepted the message. A failure that retrying cannot fix (rejected recipient,
///     invalid sender, bad credentials...) is thrown as a permanent <see cref="EmailDeliveryException"/>; any other
///     exception (timeouts, connection errors, 429, 5xx) is treated as transient and retried later.
/// </remarks>
public interface IEmailTransport
{
    Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken);
}
