using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Marketing.Domain.Model.Commands;

/// <summary>A visitor submits the demo form of the landing (US-27 scenario 1).</summary>
public record SubmitDemoRequestCommand(
    string FirstName,
    string LastName,
    string HotelName,
    string JobTitle,
    string Email,
    AccommodationType AccommodationType,
    RoomsRange RoomsRange,
    ReferralSource ReferralSource,
    VisitorProfile Profile,
    string? Phone,
    string? Message);

/// <summary>US-27 scenario 4: send the automatic follow-up to every request still waiting after the configured time.</summary>
public record SendDemoFollowUpsCommand;
