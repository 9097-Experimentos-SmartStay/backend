using System.Collections.Concurrent;
using System.Reflection;
using BackendAwSmartstay.API.Shared.Infrastructure.Mediator;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using Cortex.Mediator;
using Cortex.Mediator.Notifications;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;

public class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

    public async Task PublishAsync(IReadOnlyCollection<IEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            var eventType = @event.GetType();
            var adapterType = typeof(DomainEventNotificationAdapter<>).MakeGenericType(eventType);
            var adapter = Activator.CreateInstance(adapterType, @event);

            if (adapter is INotification notification)
            {
                var method = PublishMethods.GetOrAdd(adapterType, type =>
                    typeof(IMediator)
                        .GetMethods()
                        .First(m => m.Name == nameof(IMediator.PublishAsync) && m.IsGenericMethod)
                        .MakeGenericMethod(type));

                var task = (Task)method.Invoke(mediator, [adapter, cancellationToken])!;
                await task;
            }
        }
    }
}
