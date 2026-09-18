namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>A room.</summary>
/// <param name="Id">Room id.</param>
/// <param name="HotelId">Hotel of the room.</param>
/// <param name="RoomTypeId">Room type.</param>
/// <param name="RoomTypeName">Name of the room type.</param>
/// <param name="Price">Price per night.</param>
/// <param name="Description">Description.</param>
/// <param name="Amenities">Amenities.</param>
/// <param name="Status">Available, Occupied, Cleaning or Maintenance (US-29).</param>
public record RoomResource(
    int Id, 
    int HotelId, 
    int RoomTypeId, 
    string RoomTypeName, 
    decimal Price, 
    string Description, 
    List<string> Amenities,
    string Status
);