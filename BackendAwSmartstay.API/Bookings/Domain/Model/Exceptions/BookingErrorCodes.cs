namespace BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;

/// <summary>Stable error codes of the Bookings context (US-51, US-07; <c>check_in.*</c> is US-08, not in the backlog).</summary>
public static class BookingErrorCodes
{
    public const string RoomUnavailable = "booking.room_unavailable";
    public const string RoomUnderMaintenance = "booking.room_under_maintenance";
    public const string HotelPaymentSettingsMissing = "booking.hotel_payment_settings_missing";
    public const string OutsideHotelScope = "booking.outside_hotel_scope";
    public const string CheckOutNotAfterCheckIn = "booking.check_out_not_after_check_in";
    public const string CheckInInPast = "booking.check_in_in_past";
    public const string ConfirmationNotAllowed = "booking.confirmation_not_allowed";
    public const string CancellationNotAllowed = "booking.cancellation_not_allowed";
    public const string CancellationTooLate = "booking.cancellation_too_late";
    public const string ChangeNotAllowed = "booking.change_not_allowed";
    public const string ChangeRequiresAField = "booking.change_requires_field";
    public const string RoomOfOtherHotel = "booking.room_of_other_hotel";
    public const string PaidTotalMismatch = "booking.paid_total_mismatch";
    public const string CalendarRangeTooLong = "booking.calendar_range_too_long";
    public const string GuestNameInvalid = "booking.guest_name_invalid";
    public const string GuestEmailInvalid = "booking.guest_email_invalid";
    public const string GuestPhoneInvalid = "booking.guest_phone_invalid";
    public const string GuestAccountInvalid = "booking.guest_account_invalid";
    public const string RoomNotFound = "room.not_found";
    public const string CodeInvalid = "booking.code_invalid";

    public const string CheckInAssistanceNotAllowed = "check_in.assistance_not_allowed";
    public const string CheckInStayEnded = "check_in.stay_ended";
    public const string CheckInMessageTooLong = "check_in.message_too_long";
    public const string CheckInAlreadyCompleted = "check_in.already_completed";
    public const string CheckInBookingNotConfirmed = "check_in.booking_not_confirmed";
    public const string CheckInNotOpenYet = "check_in.not_open_yet";
    public const string CheckInDocumentRequired = "check_in.document_required";
    public const string CheckInDocumentTooLarge = "check_in.document_too_large";
    public const string CheckInDocumentFileType = "check_in.document_file_type";
    public const string CheckInNationalityInvalid = "check_in.nationality_invalid";
    public const string CheckInDniInvalid = "check_in.dni_invalid";
    public const string CheckInDniOnlyForNationals = "check_in.dni_only_for_nationals";
    public const string CheckInPassportInvalid = "check_in.passport_invalid";
    public const string CheckInForeignerCardInvalid = "check_in.foreigner_card_invalid";
    public const string CheckInForeignerCardOnlyForForeigners = "check_in.foreigner_card_only_for_foreigners";
    public const string CheckInDocumentTypeUnknown = "check_in.document_type_unknown";

    /// <summary>An invariant of a Bookings aggregate that only a programming error can break.</summary>
    public const string InternalInvariant = "booking.internal_invariant";
}
