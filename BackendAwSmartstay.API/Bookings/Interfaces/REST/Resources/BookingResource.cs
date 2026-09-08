namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>
/// Represents a booking resource for API responses, containing essential information about a booking.
/// </summary>
/// <param name="Id">The unique identifier of the booking.</param>
/// <param name="RoomId">The identifier of the room being booked.</param>
/// <param name="GuestName">The name of the guest making the booking.</param>
/// <param name="GuestEmail">The email of the guest.</param>
/// <param name="CheckInDate">The check-in date of the booking.</param>
/// <param name="CheckOutDate">The check-out date of the booking.</param>
/// <param name="Status">The current status of the booking.</param>
public record BookingResource(int Id, int RoomId, string GuestName, string GuestEmail, DateTime CheckInDate, DateTime CheckOutDate, string Status);
