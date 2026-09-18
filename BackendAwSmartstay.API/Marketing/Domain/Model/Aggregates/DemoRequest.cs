using BackendAwSmartstay.API.Marketing.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;

/// <summary>
///     A visitor of the landing asks for a demo of SmartStay (US-27). It is confirmed immediately by e-mail, the
///     sales team is notified, and if nobody follows up in time an automatic reminder is sent (scenario 4).
/// </summary>
public class DemoRequest
{
    public const int HotelNameMinLength = 2;
    public const int HotelNameMaxLength = 100;
    public const int JobTitleMinLength = 2;
    public const int JobTitleMaxLength = 60;
    public const int MessageMaxLength = 500;

    /// <summary>EF Core constructor.</summary>
    protected DemoRequest()
    {
        FirstName = LastName = Email = HotelName = JobTitle = string.Empty;
    }

    private DemoRequest(ContactDetails contact, string hotelName, string jobTitle, AccommodationType accommodationType,
        RoomsRange roomsRange, ReferralSource referralSource, VisitorProfile profile, string? message, DateTimeOffset now)
    {
        FirstName = contact.FirstName;
        LastName = contact.LastName;
        Email = contact.Email;
        Phone = contact.Phone;
        HotelName = hotelName;
        JobTitle = jobTitle;
        AccommodationType = accommodationType;
        RoomsRange = roomsRange;
        ReferralSource = referralSource;
        Profile = profile;
        Message = message;
        Status = DemoRequestStatus.Received;
        ReceivedAt = now;
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public string HotelName { get; private set; }
    public string JobTitle { get; private set; }
    public AccommodationType AccommodationType { get; private set; }
    public RoomsRange RoomsRange { get; private set; }
    public ReferralSource ReferralSource { get; private set; }
    public VisitorProfile Profile { get; private set; }
    public string? Message { get; private set; }
    public DemoRequestStatus Status { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? FollowedUpAt { get; private set; }

    public ContactDetails Contact => new(FirstName, LastName, Email, Phone);

    /// <summary>Records a new request (scenario 1).</summary>
    public static DemoRequest Submit(ContactDetails contact, string hotelName, string jobTitle,
        AccommodationType accommodationType, RoomsRange roomsRange, ReferralSource referralSource,
        VisitorProfile profile, string? message, DateTimeOffset now)
    {
        var hotel = Text(hotelName, "Hotel name", HotelNameMinLength, HotelNameMaxLength);
        var job = Text(jobTitle, "Job title", JobTitleMinLength, JobTitleMaxLength);
        var note = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        if (note is { Length: > MessageMaxLength })
            throw new DomainValidationException(MarketingErrorCodes.MessageTooLong, $"The message cannot exceed {MessageMaxLength} characters.");

        return new DemoRequest(contact, hotel, job, accommodationType, roomsRange, referralSource, profile, note, now);
    }

    /// <summary>
    ///     Scenario 4: a request still waiting (Received) for at least <paramref name="waitingTime"/> gets an
    ///     automatic follow-up.
    /// </summary>
    public bool IsDueForFollowUp(DateTimeOffset now, TimeSpan waitingTime) =>
        Status == DemoRequestStatus.Received && now - ReceivedAt >= waitingTime;

    /// <summary>Marks the follow-up as sent. Idempotent: returns false when it was already sent.</summary>
    public bool MarkFollowedUp(DateTimeOffset now)
    {
        if (Status == DemoRequestStatus.FollowedUp) return false;
        Status = DemoRequestStatus.FollowedUp;
        FollowedUpAt = now;
        return true;
    }

    private static string Text(string? value, string label, int min, int max)
    {
        var trimmed = System.Text.RegularExpressions.Regex.Replace(value?.Trim() ?? string.Empty, @"\s+", " ");
        if (trimmed.Length == 0) throw new DomainValidationException(MarketingErrorCodes.FieldRequired, $"{label} is required.");
        if (trimmed.Length < min || trimmed.Length > max)
            throw new DomainValidationException(MarketingErrorCodes.FieldLength, $"{label} must have between {min} and {max} characters.");
        return trimmed;
    }
}
