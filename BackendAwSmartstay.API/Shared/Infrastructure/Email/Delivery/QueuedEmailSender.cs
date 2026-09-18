using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Delivery;

/// <summary>
///     <see cref="IEmailSender"/> adapter: queues the message and returns immediately. The response time of the
///     request is therefore the same whether or not an e-mail is sent, and a slow or failing SMTP relay never
///     fails a use case whose changes are already committed.
/// </summary>
public class QueuedEmailSender(EmailDeliveryQueue queue) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default) =>
        queue.EnqueueAsync(message, cancellationToken).AsTask();
}
