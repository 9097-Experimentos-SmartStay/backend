namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Confirms a booking because its payment was registered (D1). Only the Payments context issues it, through the
///     Bookings ACL facade: there is no manual confirmation.
/// </summary>
/// <param name="BookingId">The identifier of the booking to confirm.</param>
public record ConfirmBookingCommand(int BookingId);
