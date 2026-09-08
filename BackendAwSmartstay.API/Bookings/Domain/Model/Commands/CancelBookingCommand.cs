namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Command to cancel a booking.
/// </summary>
/// <param name="BookingId">The identifier of the booking to cancel.</param>
public record CancelBookingCommand(int BookingId);