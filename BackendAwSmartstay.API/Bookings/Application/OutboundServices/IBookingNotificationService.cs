using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Application.OutboundServices;

/// <summary>
///     E-mails to the guest about their booking. Called by the booking event handlers, after the commit.
/// </summary>
public interface IBookingNotificationService
{
    /// <summary>US-51 scenario 2: booking received with its code, total, how to pay and the payment deadline.</summary>
    Task SendBookingPlacedAsync(Booking booking, HotelSummary? hotel, PaymentInstructions instructions);

    /// <summary>The payment was registered: the booking is confirmed.</summary>
    Task SendBookingConfirmedAsync(Booking booking, HotelSummary? hotel);

    /// <summary>US-07 scenario 4: the booking was cancelled (by the guest, the hotel or the payment deadline).</summary>
    Task SendBookingCancelledAsync(Booking booking, HotelSummary? hotel);

    /// <summary>US-07 scenario 3: the dates or the room changed.</summary>
    Task SendBookingRescheduledAsync(Booking booking, HotelSummary? hotel, DateTime previousCheckIn, DateTime previousCheckOut, int previousRoomId);
}
