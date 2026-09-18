using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Outbox;

/// <summary>
///     <see cref="IEmailSender"/> adapter of the transactional outbox: the message is added as an
///     <see cref="OutboxEmail"/> to the request's <see cref="AppDbContext"/>, i.e. enlisted in the current unit of
///     work. It is written by the same <c>SaveChanges</c>/transaction as the business change, so it is persisted
///     only if that change commits, and <see cref="OutboxEmailDispatcher"/> delivers it afterwards (surviving
///     restarts). Nothing is sent here: the request never waits for, nor reveals, the mail system.
/// </summary>
public class OutboxEmailSender(AppDbContext context, TimeProvider timeProvider) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        context.Set<OutboxEmail>().Add(OutboxEmail.Enqueue(message, timeProvider.GetUtcNow()));
        return Task.CompletedTask;
    }
}
