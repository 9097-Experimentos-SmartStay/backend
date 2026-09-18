using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

/// <summary>
///     Reacts to a domain event (e.g. the Audit context records the IAM access events). Handlers run in the scope
///     of the request that raised the event, inside the transaction of the unit of work that saved it: their writes
///     (including e-mails through <c>IEmailSender</c>, a transactional outbox) commit only with that change, and a
///     failing handler rolls it back. Never call an external system directly from a handler.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
