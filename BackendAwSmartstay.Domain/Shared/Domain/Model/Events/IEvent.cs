namespace BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

/// <summary>
/// Defines a domain event contract with occurrence timestamp.
/// </summary>
public interface IEvent
{
    DateTimeOffset OccurredOn { get; }
}

