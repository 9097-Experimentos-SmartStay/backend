using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;

/// <summary>
///     The booking does not exist, or it is not visible to the requester: a guest never learns about bookings
///     that are not theirs.
/// </summary>
public class BookingNotFoundException(int bookingId) : EntityNotFoundException("Booking", bookingId);

/// <summary>R1 / US-07: an active booking of the same room already covers some of the requested nights (overbooking).</summary>
public class RoomNotAvailableException(int roomId, DateRange dates)
    : BusinessRuleViolationException($"Room {roomId} is not available for {dates}: it overlaps an active booking.");

/// <summary>The booking's current status does not allow the requested transition.</summary>
public class InvalidBookingTransitionException(string message) : BusinessRuleViolationException(message);
