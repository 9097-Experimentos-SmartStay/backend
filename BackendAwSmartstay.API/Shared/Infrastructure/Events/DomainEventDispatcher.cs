using System.Collections.Concurrent;
using System.Reflection;
using BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Events;

/// <summary>
///     In-process dispatcher: resolves every <see cref="IDomainEventHandler{TEvent}"/> registered for the concrete
///     event type from the current scope and runs them in order.
/// </summary>
/// <remarks>
///     The unit of work dispatches inside the transaction of the change, so a failing handler is not swallowed: its
///     exception rolls the whole operation back (the change, the audit entries and the outbox e-mails alike).
/// </remarks>
public class DomainEventDispatcher(IServiceProvider serviceProvider)
    : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> DispatchMethods = new();

    private static readonly MethodInfo GenericDispatch = typeof(DomainEventDispatcher)
        .GetMethod(nameof(DispatchToHandlersAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;

    public async Task DispatchAsync(IEnumerable<IEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var method = DispatchMethods.GetOrAdd(domainEvent.GetType(), type => GenericDispatch.MakeGenericMethod(type));
            await (Task)method.Invoke(this, [domainEvent, cancellationToken])!;
        }
    }

    private async Task DispatchToHandlersAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
            await handler.HandleAsync(domainEvent, cancellationToken);
    }
}
