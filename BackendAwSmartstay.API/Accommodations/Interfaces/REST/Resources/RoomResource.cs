namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

public record RoomResource(
    int Id, 
    int HotelId, 
    int RoomTypeId, 
    string RoomTypeName, 
    decimal Price, 
    string Description, 
    List<string> Amenities
);