namespace BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

/// <summary>
///     An aggregate that records domain events. The unit of work collects them when it saves the aggregate and
///     publishes them once the changes are committed.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
