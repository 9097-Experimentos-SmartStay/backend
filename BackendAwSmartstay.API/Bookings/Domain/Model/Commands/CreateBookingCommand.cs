using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Command to place a new booking (US-51 guest, US-07 scenario 2 staff).
/// </summary>
/// <param name="Requester">Who places the booking (a guest for themselves or hotel staff on behalf of someone).</param>
/// <param name="RoomId">The identifier of the room to book.</param>
/// <param name="GuestName">Name of the guest (staff bookings; a guest defaults to their account name).</param>
/// <param name="GuestEmail">E-mail of the guest (staff bookings without an account; a guest always gets the account e-mail).</param>
/// <param name="GuestPhone">Optional phone of the guest.</param>
/// <param name="CheckInDate">The check-in date.</param>
/// <param name="CheckOutDate">The check-out date.</param>
/// <param name="UserId">Guest account the staff books for (ignored when the requester is a guest).</param>
/// <param name="GuestProfileId">Guest profile the staff books for (ignored when the requester is a guest).</param>
public record CreateBookingCommand(
    BookingRequester Requester,
    int RoomId,
    string? GuestName,
    string? GuestEmail,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int? UserId = null,
    Guid? GuestProfileId = null,
    string? GuestPhone = null);
