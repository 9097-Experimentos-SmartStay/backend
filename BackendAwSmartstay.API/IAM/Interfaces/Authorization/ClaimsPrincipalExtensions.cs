using System.Globalization;
using System.Security.Claims;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;

namespace BackendAwSmartstay.API.IAM.Interfaces.Authorization;

/// <summary>
///     Reads the authenticated requester from the <see cref="ClaimsPrincipal"/> built by the JWT bearer handler.
/// </summary>
/// <remarks>
///     Controllers use these helpers to translate the requester into explicit data of their commands and
///     queries (e.g. <c>RequesterUserId</c>); the application layer never reads the HTTP context.
/// </remarks>
public static class ClaimsPrincipalExtensions
{
    /// <summary>The authenticated user id. Only call it on endpoints that require an authenticated user.</summary>
    /// <exception cref="InvalidOperationException">The principal has no valid <c>sub</c> claim.</exception>
    public static int GetUserId(this ClaimsPrincipal principal) =>
        principal.FindUserId()
        ?? throw new InvalidOperationException("The authenticated principal has no valid 'sub' claim.");

    /// <summary>The user id, or null when the request is anonymous.</summary>
    public static int? FindUserId(this ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true
            ? ParseInt(principal.FindFirstValue(IamClaimTypes.UserId))
            : null;

    /// <summary>The username (the identity name).</summary>
    public static string GetUsername(this ClaimsPrincipal principal) =>
        principal.Identity?.Name ?? string.Empty;

    /// <summary>The hotel the user is assigned to, if any.</summary>
    public static int? GetHotelId(this ClaimsPrincipal principal) =>
        ParseInt(principal.FindFirstValue(IamClaimTypes.HotelId));

    /// <summary>The token generation the principal was issued with.</summary>
    public static int? GetTokenVersion(this ClaimsPrincipal principal) =>
        ParseInt(principal.FindFirstValue(IamClaimTypes.TokenVersion));

    /// <summary>True when the requester acts as a guest (as opposed to hotel staff).</summary>
    public static bool IsGuest(this ClaimsPrincipal principal) => principal.IsInRole(UserRoles.Guest);

    /// <summary>True when the requester is the administrator of a single hotel.</summary>
    public static bool IsHotelAdmin(this ClaimsPrincipal principal) => principal.IsInRole(UserRoles.Admin);

    /// <summary>True when the requester is a chain administrator.</summary>
    public static bool IsChainAdmin(this ClaimsPrincipal principal) => principal.IsInRole(UserRoles.ChainAdmin);

    private static int? ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
}
