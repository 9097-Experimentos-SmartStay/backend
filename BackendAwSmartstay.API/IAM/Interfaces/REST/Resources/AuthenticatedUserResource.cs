namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     A signed-in user and their tokens (sign-in and refresh responses).
/// </summary>
public record AuthenticatedUserResource
{
    /// <summary>The user's unique identifier.</summary>
    /// <example>1</example>
    public int Id { get; init; }

    /// <summary>Deprecated: same value as <see cref="Email"/> (kept for existing clients).</summary>
    /// <example>ana.perez@example.com</example>
    public string Username { get; init; } = string.Empty;

    /// <summary>The account e-mail (login identifier).</summary>
    /// <example>ana.perez@example.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>First name (null for accounts created before names were required).</summary>
    /// <example>Ana</example>
    public string? FirstName { get; init; }

    /// <summary>Last name (null for accounts created before names were required).</summary>
    /// <example>Pérez</example>
    public string? LastName { get; init; }

    /// <summary>Role that selects the dashboard: guest, reception, housekeeping, maintenance, admin or chain_admin.</summary>
    /// <example>guest</example>
    public string Role { get; init; } = string.Empty;

    /// <summary>The user's hotel, if any.</summary>
    /// <example>1</example>
    public int? HotelId { get; init; }

    /// <summary>The user's chain, if any.</summary>
    public int? ChainId { get; init; }

    /// <summary>Whether the e-mail was verified. Unverified accounts can sign in (see the contract).</summary>
    /// <example>true</example>
    public bool EmailVerified { get; init; }

    /// <summary>Access token (JWT) to send as <c>Authorization: Bearer ...</c>.</summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>Always <c>Bearer</c>.</summary>
    /// <example>Bearer</example>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>When the access token expires (UTC). Refresh it before, or sign in again.</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    ///     Refresh token of a remembered session (only with <c>rememberMe</c>, and on refresh). Single use: every
    ///     refresh returns a new one that replaces it.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>When the refresh token expires if it is not used (UTC).</summary>
    public DateTimeOffset? RefreshTokenExpiresAt { get; init; }
}
