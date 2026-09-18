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

    // Images of a hotel (hosted in the project's media library)
    public const string HotelImageUrlNotAllowed = "hotel.image_url_not_allowed";

    // Payment methods of a hotel (US-53): field rules of HotelPaymentSettings
    public const string PaymentAccountHolderRequired = "payment_settings.account_holder_required";
    public const string PaymentAccountHolderLength = "payment_settings.account_holder_length";
    public const string PaymentYapeNumberInvalid = "payment_settings.yape_number_invalid";
    public const string PaymentPlinNumberInvalid = "payment_settings.plin_number_invalid";
    public const string PaymentBankNameRequired = "payment_settings.bank_name_required";
    public const string PaymentBankNameLength = "payment_settings.bank_name_length";
    public const string PaymentBankAccountNumberRequired = "payment_settings.bank_account_number_required";
    public const string PaymentBankAccountNumberInvalid = "payment_settings.bank_account_number_invalid";
    public const string PaymentCciInvalid = "payment_settings.cci_invalid";
    public const string PaymentMethodRequired = "payment_settings.method_required";
}
