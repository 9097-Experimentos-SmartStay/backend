namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// A user account as seen by the administration (US-03).
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Username">Deprecated: same value as <paramref name="Email"/>.</param>
/// <param name="Role">The assigned role.</param>
/// <param name="Status">Active or Inactive (deactivated users keep their history).</param>
/// <param name="HotelId">The affiliated hotel, if any.</param>
/// <param name="ChainId">The affiliated chain, if any.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
/// <param name="Email">The account e-mail (login identifier).</param>
/// <param name="FirstName">First name (null for accounts created before names were required).</param>
/// <param name="LastName">Last name (null for accounts created before names were required).</param>
/// <param name="EmailVerified">Whether the e-mail was verified.</param>
/// <param name="LockedUntil">End of the temporary lock after failed sign-ins, if one is in effect or recent.</param>
public record UserResource(
    int Id,
    string Username,
    string Role,
    string Status,
    int? HotelId,
    int? ChainId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified,
    DateTimeOffset? LockedUntil,
    bool MfaEnabled,
    bool MfaEnrollmentRequired);
