using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;

/**
 * This attribute is used to decorate controllers and actions that require authorization.
 * It checks if the user is logged in by checking if HttpContext.User is set.
 * If a user is not signed in, then it returns a 401-status code.
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    /**
     * <summary>
     *     This method is called when authorization is required.
     *     It checks if the user is logged in by checking if HttpContext.User is set.
     *     If a user is not signed in then it returns 401-status code.
     * </summary>
     * <param name="context">The authorization filter context</param>
     */
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        var user = (User?)context.HttpContext.Items["User"];

        if (user == null)
        {
            context.Result = Problem(StatusCodes.Status401Unauthorized, "Unauthorized",
                "A valid bearer token is required.");
            return;
        }

        if (_roles.Length == 0) return;

        var hasAllowedRole = _roles.Any(role =>
            string.Equals(role, user.Role, StringComparison.OrdinalIgnoreCase));

        // ForbidResult would require ASP.NET authentication handlers (not registered in this app)
        // and ended up as a 500, so the 403 is written directly.
        if (!hasAllowedRole)
            context.Result = Problem(StatusCodes.Status403Forbidden, "Forbidden",
                $"Role '{user.Role.Value}' is not allowed to perform this operation.");
    }

    private static ObjectResult Problem(int statusCode, string title, string detail) =>
        new(new ProblemDetails { Status = statusCode, Title = title, Detail = detail })
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
}