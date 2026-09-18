namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;

/// <summary>Stable error codes of the Accommodations context (hotels, rooms, room types, catalogs; US-53, US-06).</summary>
public static class AccommodationErrorCodes
{
    public const string AdminAlreadyHasHotel = "hotel.admin_already_has_hotel";
    public const string HotelHasActiveBookings = "hotel.has_active_bookings";
    public const string HotelNotFound = "hotel.not_found";
    public const string HotelRequired = "hotel.required";
    public const string RoomNumberTaken = "room.number_taken";
    public const string RoomNumberInvalid = "room.number_invalid";
    public const string RoomPriceOutOfRange = "room.price_out_of_range";
    public const string RoomHasActiveBookings = "room.has_active_bookings";
    public const string RoomInvalidStatusTransition = "room.invalid_status_transition";
    public const string RoomNotReady = "room.not_ready";
    public const string RoomTypeNotFound = "room_type.not_found";
    public const string CategoryAlreadyExists = "catalog.category_already_exists";
    public const string AmenityAlreadyExists = "catalog.amenity_already_exists";
}
