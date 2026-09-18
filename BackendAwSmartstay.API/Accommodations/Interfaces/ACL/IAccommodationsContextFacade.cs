namespace BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

public interface IAccommodationsContextFacade
{
    Task<bool> HotelExistsAsync(int hotelId);

    /// <summary>True when the room exists.</summary>
    Task<bool> RoomExistsAsync(int roomId);

    /// <summary>The room's price per night, or null when the room does not exist.</summary>
    Task<decimal?> FetchRoomPricePerNightAsync(int roomId);
}
