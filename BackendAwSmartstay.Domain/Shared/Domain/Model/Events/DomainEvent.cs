namespace BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

/// <summary>Base record of a domain event: <see cref="OccurredOn"/> is the moment the fact happened (UTC).</summary>
public abstract record DomainEvent(DateTimeOffset OccurredOn) : IEvent;
