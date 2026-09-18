using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Command to cancel a booking on behalf of <paramref name="Requester"/>.
/// </summary>
/// <param name="BookingId">The identifier of the booking to cancel.</param>
/// <param name="Requester">Who cancels (a guest can only cancel their own booking).</param>
public record CancelBookingCommand(int BookingId, BookingRequester Requester);
