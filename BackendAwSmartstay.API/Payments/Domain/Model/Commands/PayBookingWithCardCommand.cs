using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Payments.Domain.Model.Commands;

/// <summary>
///     A guest pays their own Pending booking online with a card. The amount is not part of the command: it is always
///     the booking's total.
/// </summary>
/// <param name="BookingId">The booking paid.</param>
/// <param name="GuestUserId">The guest paying; the booking must be theirs.</param>
/// <param name="Card">The card entered (kept in memory only).</param>
public record PayBookingWithCardCommand(int BookingId, int GuestUserId, CardDetails Card);
