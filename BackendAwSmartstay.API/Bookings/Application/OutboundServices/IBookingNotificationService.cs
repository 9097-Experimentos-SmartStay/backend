using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Bookings.Application.OutboundServices;

/// <summary>Where a booking is: its hotel and the number of its room (as guests and staff know it).</summary>
public sealed record BookingPlace(HotelSummary? Hotel, string RoomNumber);

/// <summary>
///     E-mails to the guest about their booking. Called by the booking event handlers, after the commit.
/// </summary>
public interface IBookingNotificationService
{
    /// <summary>
    ///     US-51 scenario 2: booking received with its code, total, how to pay (the hotel's payment methods; null
    ///     only for a hotel that has none, e.g. a booking made before they were required) and the payment deadline.
    /// </summary>
    Task SendBookingPlacedAsync(Booking booking, BookingPlace place, HotelPaymentInstructions? instructions);

    /// <summary>The payment was registered: the booking is confirmed.</summary>
    Task SendBookingConfirmedAsync(Booking booking, BookingPlace place);

    /// <summary>US-07 scenario 4: the booking was cancelled (by the guest, the hotel or the payment deadline).</summary>
    Task SendBookingCancelledAsync(Booking booking, BookingPlace place);

    /// <summary>US-07 scenario 3: the dates or the room changed.</summary>
    Task SendBookingRescheduledAsync(Booking booking, BookingPlace place, DateTime previousCheckIn, DateTime previousCheckOut, string previousRoomNumber);
}
