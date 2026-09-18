using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.Shared.Application.OutboundServices;

/// <summary>
///     Delivers domain events to their <c>IDomainEventHandler</c>s. The unit of work calls it for the events of the
///     aggregates it saves, inside their transaction; application services call it directly for facts that change
///     no aggregate (e.g. a sign-in attempt with an unknown e-mail). A handler failure is propagated.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IEvent> domainEvents, CancellationToken cancellationToken = default);
}
