namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

public record CreateRoomResource(
    int HotelId, 
    int RoomTypeId, 
    decimal Price, 
    string Description, 
    List<string> Amenities
);