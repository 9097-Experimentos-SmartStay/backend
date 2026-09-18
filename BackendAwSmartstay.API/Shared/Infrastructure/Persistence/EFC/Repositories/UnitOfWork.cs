using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Implementation of the unit of work pattern using Entity Framework Core.
/// </summary>
/// <remarks>
///     Domain events recorded by the saved aggregates (<see cref="IHasDomainEvents"/>) are published only once the
///     changes are durable: right after <c>SaveChanges</c>, or after the commit when the work runs inside
///     <see cref="ExecuteInTransactionAsync"/>. A rolled-back transaction publishes nothing.
/// </remarks>
public class UnitOfWork(AppDbContext context, IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    private readonly List<IEvent> _committedEvents = [];

    /// <inheritdoc/>
    public async Task CompleteAsync()
    {
        // Taken before saving (deleted entities are detached by SaveChanges), read after it (new aggregates
        // have their generated ids by then).
        var aggregates = context.ChangeTracker.Entries<IHasDomainEvents>().Select(entry => entry.Entity).ToList();

        await context.SaveChangesAsync();

        var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());
        _committedEvents.AddRange(events);

        if (context.Database.CurrentTransaction is null)
            await PublishCommittedEventsAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> work)
    {
        if (context.Database.CurrentTransaction is not null)
        {
            await work();
            return;
        }

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                await work();
                await transaction.CommitAsync();
            }
            catch
            {
                _committedEvents.Clear();
                throw;
            }
        }

        await PublishCommittedEventsAsync();
    }

    private async Task PublishCommittedEventsAsync()
    {
        if (_committedEvents.Count == 0) return;

        // Handlers may save through this same unit of work: take the events out first (no re-publication).
        var events = _committedEvents.ToList();
        _committedEvents.Clear();
        await domainEventDispatcher.DispatchAsync(events);
    }
}
