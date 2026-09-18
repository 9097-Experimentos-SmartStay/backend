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
///     Events are dispatched after the commit, so a failing handler must not turn a committed operation into an
///     error response: the failure is logged and the remaining handlers still run.
/// </remarks>
public class DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
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
        {
            try
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Handler {Handler} failed for domain event {Event}.",
                    handler.GetType().Name, typeof(TEvent).Name);
            }
        }
    }
}
