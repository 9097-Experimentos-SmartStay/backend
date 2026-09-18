using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;

/// <summary>US-53: room numbers are unique within a hotel.</summary>
public class DuplicateRoomNumberException(string number, int hotelId)
    : BusinessRuleViolationException($"Room number {number} already exists in hotel {hotelId}. Use another number.");

/// <summary>A room (or a hotel with rooms) that still holds active bookings cannot be deleted.</summary>
public class RoomHasActiveBookingsException(string message) : BusinessRuleViolationException(message);
