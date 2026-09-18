using BackendAwSmartstay.API.IAM.Domain.Model.Enums;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Whether an access token presented by a user still represents a valid session. The token's claims (role,
///     hotel, chain) are the authority for authorization: any change of them ends the user's sessions (a new token
///     generation), so a token that is still valid always carries current permissions (US-03 scenario 2).
/// </summary>
/// <param name="Status">Valid, or why the token is not accepted.</param>
/// <param name="Role">Current role (valid sessions only; informative).</param>
/// <param name="HotelId">Current hotel (valid sessions only; informative).</param>
/// <param name="ChainId">Current chain (valid sessions only; informative).</param>
/// <param name="RevocationReason">Why the sessions ended, when the status is Revoked or Inactive and it is known.</param>
public sealed record UserSession(
    UserSessionStatus Status,
    string? Role = null,
    int? HotelId = null,
    int? ChainId = null,
    SessionRevocationReason? RevocationReason = null)
{
    public bool IsValid => Status == UserSessionStatus.Valid;

    public static UserSession NotFound() => new(UserSessionStatus.UserNotFound);
}
