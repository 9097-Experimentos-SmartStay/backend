using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

/// <summary>
///     Reacts to a domain event after the unit of work that produced it has been committed (e.g. the Audit
///     context records the IAM access events). Handlers run in the scope of the request that raised the event.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
