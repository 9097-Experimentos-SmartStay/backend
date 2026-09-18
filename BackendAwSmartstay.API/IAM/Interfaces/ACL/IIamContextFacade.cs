namespace BackendAwSmartstay.API.IAM.Interfaces.ACL;

/// <summary>How to reach a user, exposed to other bounded contexts.</summary>
/// <param name="UserId">IAM user id.</param>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">First and last name, or null for accounts created before names were required.</param>
/// <param name="Role">Role of the account.</param>
public sealed record UserContact(int UserId, string Email, string? FullName, string Role);

/// <summary>The requester's current session, as seen by other contexts (opaque: only IAM interprets it).</summary>
/// <param name="RememberedSessionId">The remembered session ("remember me") of the access token, if any.</param>
public sealed record SessionContext(Guid? RememberedSessionId);

/// <summary>New credentials issued by IAM on the user's behalf (OWASP: new credentials on a privilege change).</summary>
/// <param name="AccessToken">Bearer access token with the new permissions.</param>
/// <param name="AccessTokenExpiresAt">When it expires.</param>
/// <param name="RefreshToken">Refresh token, only when the previous session was a remembered one.</param>
/// <param name="RefreshTokenExpiresAt">When the refresh token expires.</param>
public sealed record ReissuedSession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string? RefreshToken,
    DateTimeOffset? RefreshTokenExpiresAt);

public interface IIamContextFacade
{
    /// <summary>The contact of an active user, or null when the user does not exist or is inactive.</summary>
    Task<UserContact?> FetchUserContactAsync(int userId);

    /// <summary>
    ///     Active users of <paramref name="hotelId"/> with one of <paramref name="roles"/> (e.g. the housekeeping staff
    ///     of a hotel to notify). Chain administrators are only included when <paramref name="roles"/> asks for them.
    /// </summary>
    Task<IReadOnlyList<UserContact>> ListHotelStaffAsync(int hotelId, IReadOnlyCollection<string> roles);

    Task<int> FetchUserIdByEmail(string email);
    Task<string> FetchEmailByUserId(int userId);

    /// <summary>
    ///     D2: makes <paramref name="hotelId"/> the hotel administered by the hotel administrator
    ///     <paramref name="userId"/> (called when that administrator registers their hotel). Their previous access
    ///     and refresh tokens are revoked and, because the change is theirs, a new session with the hotel is
    ///     returned (remembered when <paramref name="currentSession"/> was). Runs in the caller's transaction.
    /// </summary>
    /// <returns>The new session, or null when the administrator already had that hotel.</returns>
    Task<ReissuedSession?> AssignHotelToAdministratorAsync(int userId, int hotelId, SessionContext currentSession);
}