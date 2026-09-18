using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;

/// <summary>Booking rules that depend on the operation (section <c>Bookings</c>, env vars <c>Bookings__*</c>).</summary>
public class BookingPolicySettings : IValidatableObject
{
    public const string SectionName = "Bookings";

    /// <summary>
    ///     IANA time zone of the hotels: decides "today" for the date rules (no check-in in the past, no cancellation
    ///     on the check-in day, check-in window).
    /// </summary>
    [Required]
    public string TimeZone { get; set; } = "America/Lima";

    /// <summary>Hours a Pending booking waits for its payment before it expires (payment hold, D1).</summary>
    [Range(1, 168)]
    public int PaymentHoldHours { get; set; } = 24;

    /// <summary>Check-out time (hotel time, HH:mm): the room access code stops working then (US-08).</summary>
    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Bookings:CheckOutTime must be HH:mm.")]
    public string CheckOutTime { get; set; } = "12:00";

    public TimeSpan PaymentHold => TimeSpan.FromHours(PaymentHoldHours);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(TimeZone, out _))
            yield return new ValidationResult($"Bookings:TimeZone '{TimeZone}' is not a known time zone.", [nameof(TimeZone)]);
    }
}
