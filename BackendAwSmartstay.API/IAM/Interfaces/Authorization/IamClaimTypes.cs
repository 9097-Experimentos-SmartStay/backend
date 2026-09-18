using Microsoft.IdentityModel.JsonWebTokens;

namespace BackendAwSmartstay.API.IAM.Interfaces.Authorization;

/// <summary>
///     Claim names carried by the SmartStay access token (published language of the IAM bounded context).
/// </summary>
/// <remarks>
///     Inbound claim mapping is disabled, so these short JWT names are exactly what both the API and the
///     clients read from the token. Clients should keep using the sign-in response for display data and only
///     rely on these claims when they decode the token.
/// </remarks>
public static class IamClaimTypes
{
    /// <summary>User id (JWT <c>sub</c>).</summary>
    public const string UserId = JwtRegisteredClaimNames.Sub;

    /// <summary>
    ///     Login identifier (JWT <c>unique_name</c>, the account e-mail). Used as
    ///     <see cref="System.Security.Claims.ClaimsIdentity.Name"/>.
    /// </summary>
    public const string Username = JwtRegisteredClaimNames.UniqueName;

    /// <summary>Account e-mail (JWT <c>email</c>).</summary>
    public const string Email = JwtRegisteredClaimNames.Email;

    /// <summary>Role (<c>role</c>): guest, reception, housekeeping, maintenance, admin or chain_admin.</summary>
    public const string Role = "role";

    /// <summary>Hotel the user is assigned to (<c>hotel_id</c>), only present when assigned.</summary>
    public const string HotelId = "hotel_id";

    /// <summary>Chain the user belongs to (<c>chain_id</c>), only present when assigned.</summary>
    public const string ChainId = "chain_id";

    /// <summary>Session generation (<c>token_version</c>); a token whose version is not the current one is revoked.</summary>
    public const string TokenVersion = "token_version";
}
