namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>
///     A registered hotel (US-53 scenario 1) and, when a hotel administrator registered their own hotel, their new
///     session: it carries the hotel (<c>hotel_id</c>) and replaces the previous one, whose tokens were revoked.
/// </summary>
/// <param name="Hotel">The new hotel.</param>
/// <param name="Session">The administrator's new session; null for a chain administrator.</param>
public record HotelRegistrationResource(HotelResource Hotel, RenewedSessionResource? Session);

/// <summary>New credentials issued because the user's permissions changed at their own request.</summary>
/// <param name="Token">Bearer access token with the new permissions (use it from now on).</param>
/// <param name="TokenType">Always <c>Bearer</c>.</param>
/// <param name="ExpiresAt">When the access token expires.</param>
/// <param name="RefreshToken">New refresh token, only when the previous session was remembered ("remember me").</param>
/// <param name="RefreshTokenExpiresAt">When the refresh token expires.</param>
public record RenewedSessionResource(
    string Token,
    string TokenType,
    DateTimeOffset ExpiresAt,
    string? RefreshToken,
    DateTimeOffset? RefreshTokenExpiresAt);
