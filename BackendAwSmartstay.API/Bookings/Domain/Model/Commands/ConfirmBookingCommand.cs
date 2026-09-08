namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Command to confirm a booking.
/// </summary>
/// <param name="BookingId">The identifier of the booking to confirm.</param>
public record ConfirmBookingCommand(int BookingId);