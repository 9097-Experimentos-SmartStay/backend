namespace BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

/// <summary>A room as offered for booking, exposed to other bounded contexts (no domain entity leaks).</summary>
/// <param name="RoomId">The room.</param>
/// <param name="HotelId">Its hotel.</param>
/// <param name="RoomTypeId">Its room type.</param>
/// <param name="RoomTypeName">Name of the room type.</param>
/// <param name="PricePerNight">Price per night.</param>
/// <param name="Description">Description.</param>
/// <param name="Amenities">Amenities.</param>
/// <param name="Status">Current operational status (Available, Occupied or Cleaning).</param>
public sealed record RoomOffer(int RoomId, int HotelId, int RoomTypeId, string RoomTypeName, decimal PricePerNight,
    string Description, IReadOnlyList<string> Amenities, string Status);

public interface IAccommodationsContextFacade
{
    Task<bool> HotelExistsAsync(int hotelId);

    /// <summary>True when the room exists.</summary>
    Task<bool> RoomExistsAsync(int roomId);

    /// <summary>The room's price per night, or null when the room does not exist.</summary>
    Task<decimal?> FetchRoomPricePerNightAsync(int roomId);

    /// <summary>The hotel the room belongs to, or null when the room does not exist.</summary>
    Task<int?> FetchHotelIdOfRoomAsync(int roomId);

    /// <summary>
    ///     Locks the room for the current transaction so no other booking of it can be created concurrently (R1).
    ///     Returns false when the room does not exist. Call it inside the booking transaction, before checking
    ///     availability.
    /// </summary>
    Task<bool> LockRoomForBookingAsync(int roomId);

    /// <summary>Rooms that can be offered for booking (not under maintenance), of one hotel or of every hotel.</summary>
    Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? hotelId);
}
