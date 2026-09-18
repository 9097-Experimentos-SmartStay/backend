namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     The signed-in user as the account is now (<c>GET /users/me</c>): clients refresh their session with it so a
///     role, hotel or chain changed by an administrator shows up immediately (US-03 scenario 2).
/// </summary>
/// <param name="Id">The user's unique identifier.</param>
/// <param name="Email">The account e-mail (login identifier).</param>
/// <param name="FirstName">First name (null for accounts created before names were required).</param>
/// <param name="LastName">Last name (null for accounts created before names were required).</param>
/// <param name="Role">guest, reception, housekeeping, maintenance, admin or chain_admin.</param>
/// <param name="HotelId">The user's hotel, if any.</param>
/// <param name="ChainId">The user's chain, if any.</param>
/// <param name="EmailVerified">Whether the e-mail was verified.</param>
/// <param name="MfaEnabled">Whether the account has an authenticator app (US-52).</param>
public record CurrentUserResource(
    int Id,
    string Email,
    string? FirstName,
    string? LastName,
    string Role,
    int? HotelId,
    int? ChainId,
    bool EmailVerified,
    bool MfaEnabled);
