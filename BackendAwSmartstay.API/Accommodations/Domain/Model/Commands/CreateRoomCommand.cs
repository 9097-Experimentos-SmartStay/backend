namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>US-53: create a room of a hotel.</summary>
/// <param name="HotelId">The hotel.</param>
/// <param name="RoomTypeId">Its room type.</param>
/// <param name="Price">Price per night (&gt; 0).</param>
/// <param name="Description">Description.</param>
/// <param name="Amenities">Amenities.</param>
/// <param name="Number">Room number, unique in the hotel.</param>
public record CreateRoomCommand(
    int HotelId,
    int RoomTypeId,
    decimal Price,
    string Description,
    List<string> Amenities,
    string Number);
