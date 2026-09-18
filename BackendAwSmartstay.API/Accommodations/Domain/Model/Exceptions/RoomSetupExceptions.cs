using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;

/// <summary>US-53: room numbers are unique within a hotel.</summary>
public class DuplicateRoomNumberException : BusinessRuleViolationException
{
    public DuplicateRoomNumberException(string number, int hotelId)
        : base(AccommodationErrorCodes.RoomNumberTaken, $"Room number {number} already exists in hotel {hotelId}. Use another number.")
    {
        Parameters = new Dictionary<string, object?> { ["number"] = number, ["hotelId"] = hotelId };
    }
}

/// <summary>A room (or a hotel with rooms) that still holds active bookings cannot be deleted.</summary>
/// <remarks>Code <c>room.has_active_bookings</c> or <c>hotel.has_active_bookings</c>; <c>activeBookings</c> holds the count.</remarks>
public class RoomHasActiveBookingsException : BusinessRuleViolationException
{
    public RoomHasActiveBookingsException(string code, string message, int activeBookings) : base(code, message)
    {
        Parameters = new Dictionary<string, object?> { ["activeBookings"] = activeBookings };
    }
}
