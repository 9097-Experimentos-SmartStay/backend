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
    : BusinessRuleViolationException(BookingErrorCodes.RoomUnavailable, $"Room {roomId} is no longer available for {dates}: another booking holds some of those nights. Search again for available rooms.");

/// <summary>A room under maintenance is never booked (US-51, US-06).</summary>
public class RoomUnderMaintenanceException(int roomId)
    : BusinessRuleViolationException(BookingErrorCodes.RoomUnderMaintenance, $"Room {roomId} is under maintenance and cannot be booked. Search again for available rooms.");

/// <summary>
///     US-53: a hotel accepts bookings only once guests know how to pay them (its administrator set at least one
///     payment method). <c>params.hotelId</c> names the hotel.
/// </summary>
public class HotelNotAcceptingBookingsException : BusinessRuleViolationException
{
    public HotelNotAcceptingBookingsException(int hotelId)
        : base(BookingErrorCodes.HotelPaymentSettingsMissing,
            $"Hotel {hotelId} does not accept bookings yet: its administrator has not set up the payment methods.")
    {
        Parameters = new Dictionary<string, object?> { ["hotelId"] = hotelId };
    }
}

/// <summary>The staff member works for another hotel (R4).</summary>
public class BookingOutsideHotelScopeException()
    : BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions.OperationNotAllowedException(BookingErrorCodes.OutsideHotelScope, "You can only manage the bookings of your hotel.");

/// <summary>The booking's current status does not allow the requested transition.</summary>
public class InvalidBookingTransitionException(string code, string message) : BusinessRuleViolationException(code, message);
