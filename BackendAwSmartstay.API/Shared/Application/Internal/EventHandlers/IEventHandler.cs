using BackendAwSmartstay.API.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace BackendAwSmartstay.API.Shared.Application.Internal.EventHandlers;

/// <summary>
/// Defines a handler for domain events.
/// </summary>
/// <typeparam name="TEvent">The type of the event to handle.</typeparam>
public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}