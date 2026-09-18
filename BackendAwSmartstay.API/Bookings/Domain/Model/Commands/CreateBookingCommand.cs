using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Command to create a new booking.
/// </summary>
/// <param name="Requester">Who places the booking (a guest for themselves or hotel staff on behalf of someone).</param>
/// <param name="RoomId">The identifier of the room to book.</param>
/// <param name="GuestName">The name of the guest.</param>
/// <param name="GuestEmail">The email of the guest.</param>
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
    Guid? GuestProfileId = null);
