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
///     Domain events recorded by the saved aggregates (<see cref="IHasDomainEvents"/>) are published right after
///     <c>SaveChanges</c>, INSIDE the database transaction of the change (opened here when the caller has none):
///     their handlers' own writes (audit entries, refunds, the e-mails of the transactional outbox) commit or roll
///     back together with the change that raised them. The new aggregates already have their generated ids when
///     the handlers run. A failing handler fails the whole operation: nothing is half-committed.
/// </remarks>
public class UnitOfWork(AppDbContext context, IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    /// <inheritdoc/>
    public async Task CompleteAsync()
    {
        // Without events a single SaveChanges is already atomic (outbox e-mails enlisted by the caller included).
        if (context.Database.CurrentTransaction is null && HasPendingDomainEvents())
            await ExecuteInTransactionAsync(SaveAndPublishAsync);
        else
            await SaveAndPublishAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> work)
    {
        if (context.Database.CurrentTransaction is not null || !context.Database.IsRelational())
        {
            await work();
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        await work();
        await transaction.CommitAsync();
    }

    private bool HasPendingDomainEvents() =>
        context.ChangeTracker.Entries<IHasDomainEvents>().Any(entry => entry.Entity.DomainEvents.Count > 0);

    /// <summary>
    ///     Saves, then publishes the events of the saved aggregates; repeats while the handlers leave new events or
    ///     unsaved changes (e.g. e-mails they enlisted in the outbox without saving).
    /// </summary>
    private async Task SaveAndPublishAsync()
    {
        while (true)
        {
            // Taken before saving (deleted entities are detached by SaveChanges), read after it (new aggregates
            // have their generated ids by then).
            var aggregates = context.ChangeTracker.Entries<IHasDomainEvents>().Select(entry => entry.Entity).ToList();

            await context.SaveChangesAsync();

            // Taken out before publishing: handlers may save through this same unit of work (no re-publication).
            var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());
            if (events.Count == 0) return;

            await domainEventDispatcher.DispatchAsync(events);
        }
    }
}
