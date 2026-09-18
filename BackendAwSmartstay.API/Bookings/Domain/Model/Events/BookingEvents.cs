using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Events;

// Published language of the Bookings context. Handlers run after the unit of work commits.

/// <summary>A booking was created as Pending, waiting for its payment until <paramref name="PaymentDueAt"/>.</summary>
public sealed record BookingCreatedEvent(int BookingId, string Code, int HotelId, int RoomId, DateTimeOffset PaymentDueAt, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A booking was confirmed (its payment was registered, D1).</summary>
public sealed record BookingConfirmedEvent(int BookingId, string Code, int HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A booking was cancelled; its nights are free again. <paramref name="WasPaid"/>: it was Confirmed (paid).</summary>
public sealed record BookingCancelledEvent(int BookingId, string Code, int HotelId, CancellationReason Reason, bool WasPaid, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>The dates or the room of a booking changed (US-07 scenario 3).</summary>
public sealed record BookingRescheduledEvent(int BookingId, string Code, int HotelId, int PreviousRoomId, DateTime PreviousCheckIn, DateTime PreviousCheckOut, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>The guest completed the digital check-in (US-08): the stay started in <paramref name="RoomId"/>.</summary>
public sealed record GuestCheckedInEvent(int BookingId, string Code, int HotelId, int RoomId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);
