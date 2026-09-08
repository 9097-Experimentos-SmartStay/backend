namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>
///     Command to create a new booking.
/// </summary>
/// <param name="RoomId">The identifier of the room to book.</param>
/// <param name="GuestName">The name of the guest.</param>
/// <param name="GuestEmail">The email of the guest.</param>
/// <param name="CheckInDate">The check-in date.</param>
/// <param name="CheckOutDate">The check-out date.</param>
public record CreateBookingCommand(
    int RoomId,
    string GuestName,
    string GuestEmail,
    DateTime CheckInDate,
    DateTime CheckOutDate);