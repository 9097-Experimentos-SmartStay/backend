namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>
/// Represents a resource for creating a new booking, containing the necessary data for booking creation.
/// </summary>
/// <param name="RoomId">The identifier of the room to book.</param>
/// <param name="GuestName">The name of the guest.</param>
/// <param name="GuestEmail">The email of the guest.</param>
/// <param name="CheckInDate">The check-in date.</param>
/// <param name="CheckOutDate">The check-out date.</param>
public record CreateBookingResource(int RoomId, string GuestName, string GuestEmail, DateTime CheckInDate, DateTime CheckOutDate);
