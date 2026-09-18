using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Extensions;

/// <summary>
///     Helpers to read the authenticated user that <c>RequestAuthorizationMiddleware</c> stores in
///     <c>HttpContext.Items["User"]</c>.
/// </summary>
public static class HttpContextUserExtensions
{
    /// <summary>Returns the authenticated user or null for anonymous requests.</summary>
    public static User? GetAuthenticatedUser(this HttpContext? httpContext) =>
        httpContext?.Items["User"] as User;

    /// <summary>
    ///     Returns the authenticated user. Only call it from endpoints protected by <c>[Authorize]</c>;
    ///     a missing user there is a programming error, surfaced as 403 instead of granting access.
    /// </summary>
    public static User RequireAuthenticatedUser(this HttpContext? httpContext) =>
        httpContext.GetAuthenticatedUser()
        ?? throw new OperationNotAllowedException("An authenticated user is required for this operation.");

    /// <summary>True when the user's role matches any of the given roles (case-insensitive).</summary>
    public static bool IsInRole(this User user, params string[] roles) =>
        roles.Any(role => string.Equals(role, user.Role.Value, StringComparison.OrdinalIgnoreCase));
}
