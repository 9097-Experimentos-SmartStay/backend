namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>A room free for the requested stay, with its price (US-29 scenario 1).</summary>
/// <param name="Id">Room id (use it as <c>roomId</c> in <c>POST /bookings</c>).</param>
/// <param name="HotelId">Hotel of the room.</param>
/// <param name="RoomTypeId">Room type.</param>
/// <param name="RoomTypeName">Name of the room type.</param>
/// <param name="Description">Description.</param>
/// <param name="Amenities">Amenities.</param>
/// <param name="Status">Current status (Available, Occupied or Cleaning; rooms in Maintenance are never offered).</param>
/// <param name="Available">Always true: only rooms free for the whole stay are returned.</param>
/// <param name="PricePerNight">Price per night.</param>
/// <param name="Nights">Nights of the stay.</param>
/// <param name="TotalPrice">PricePerNight × Nights.</param>
/// <param name="CheckInDate">Check-in date of the stay.</param>
/// <param name="CheckOutDate">Check-out date of the stay.</param>
public record AvailableRoomResource(
    int Id,
    int HotelId,
    int RoomTypeId,
    string RoomTypeName,
    string Description,
    IReadOnlyList<string> Amenities,
    string Status,
    bool Available,
    decimal PricePerNight,
    int Nights,
    decimal TotalPrice,
    DateOnly CheckInDate,
    DateOnly CheckOutDate);
