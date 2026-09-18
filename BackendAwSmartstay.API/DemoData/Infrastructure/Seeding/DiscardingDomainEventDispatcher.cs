using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.DemoData.Infrastructure.Seeding;

/// <summary>
///     Domain event dispatcher of the demo data unit of work: it drops the events instead of publishing them.
/// </summary>
/// <remarks>
///     The demo dataset is a snapshot of a past that never happened (bookings placed days ago, a payment registered
///     yesterday). Its aggregates record events as usual, but publishing them would turn that fake past into real
///     side effects now: booking and account e-mails to the demo inboxes, maintenance alerts, audit entries. The seeder
///     applies those consequences to the aggregates itself (e.g. refunding the payment of the cancelled booking), so
///     nothing depends on the handlers.
/// </remarks>
public class DiscardingDomainEventDispatcher(ILogger logger) : IDomainEventDispatcher
{
    public int DiscardedCount { get; private set; }

    public Task DispatchAsync(IEnumerable<IEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var events = domainEvents.ToList();
        DiscardedCount += events.Count;
        foreach (var domainEvent in events)
            logger.LogDebug("DemoData: domain event {Event} not published (seeded history).", domainEvent.GetType().Name);
        return Task.CompletedTask;
    }
}
