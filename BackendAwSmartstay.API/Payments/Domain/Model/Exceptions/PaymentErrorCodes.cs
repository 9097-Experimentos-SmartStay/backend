namespace BackendAwSmartstay.API.Payments.Domain.Model.Exceptions;

/// <summary>Stable error codes of the Payments context (payments registered by the hotel, US-07 scenario 5).</summary>
public static class PaymentErrorCodes
{
    public const string OperationNumberRequired = "payment.operation_number_required";
    public const string OperationNumberTooLong = "payment.operation_number_too_long";
    public const string NoteTooLong = "payment.note_too_long";
    public const string OutsideHotelScope = "payment.outside_hotel_scope";
    public const string BookingAlreadyPaid = "payment.booking_already_paid";
    public const string BookingNotPending = "payment.booking_not_pending";
    public const string CannotComplete = "payment.cannot_complete";
    public const string CannotFail = "payment.cannot_fail";

    /// <summary>An invariant of the Payment aggregate that only a programming error can break.</summary>
    public const string InternalInvariant = "payment.internal_invariant";
}
