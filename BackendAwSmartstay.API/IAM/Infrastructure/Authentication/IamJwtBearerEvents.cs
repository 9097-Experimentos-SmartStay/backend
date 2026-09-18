using System.Globalization;
using System.Security.Claims;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Authentication;

/// <summary>
///     JWT bearer events of the IAM context:
///     <list type="bullet">
///         <item>after the signature/lifetime checks, asks the IAM application layer whether the session is still
///         valid (active user, current token version: a password change or a deactivation revokes tokens) and
///         refreshes the role/scope claims from the User aggregate;</item>
///         <item>writes 401/403 responses as RFC 7807 ProblemDetails through the native problem details service.</item>
///     </list>
/// </summary>
public class IamJwtBearerEvents(
    IUserQueryService userQueryService,
    IProblemDetailsService problemDetailsService,
    ILogger<IamJwtBearerEvents> logger) : JwtBearerEvents
{
    private const string MissingTokenDetail = "A valid bearer token is required to access this resource.";
    private const string InvalidTokenDetail = "The bearer token is invalid.";
    private const string ExpiredTokenDetail = "The bearer token has expired. Sign in again.";
    private const string RevokedTokenDetail = "The bearer token has been revoked. Sign in again.";
    private const string InactiveUserDetail = "The account has been deactivated. Contact the administrator.";
    private const string ForbiddenDetail = "You do not have permission to perform this operation.";

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal!;
        var userId = principal.FindUserId();
        var tokenVersion = principal.GetTokenVersion();

        if (userId is null || tokenVersion is null)
        {
            context.Fail(InvalidTokenDetail);
            return;
        }

        var session = await userQueryService.Handle(new GetUserSessionQuery(userId.Value, tokenVersion.Value));
        switch (session.Status)
        {
            case UserSessionStatus.Valid:
                RefreshAuthorizationClaims((ClaimsIdentity)principal.Identity!, session);
                return;
            case UserSessionStatus.Inactive:
                logger.LogInformation("Rejected token of inactive user {UserId}.", userId);
                context.Fail(InactiveUserDetail);
                return;
            case UserSessionStatus.UserNotFound:
                logger.LogWarning("Rejected token of unknown user {UserId}.", userId);
                context.Fail(RevokedTokenDetail);
                return;
            default:
                logger.LogInformation("Rejected revoked token (version {TokenVersion}) of user {UserId}.", tokenVersion, userId);
                context.Fail(RevokedTokenDetail);
                return;
        }
    }

    /// <summary>
    ///     Role and scope are read from the User aggregate on every request, so an administrator's change
    ///     (role, hotel, chain) applies immediately even to tokens issued before it.
    /// </summary>
    private static void RefreshAuthorizationClaims(ClaimsIdentity identity, UserSession session)
    {
        Replace(identity, IamClaimTypes.Role, session.Role);
        Replace(identity, IamClaimTypes.HotelId, session.HotelId?.ToString(CultureInfo.InvariantCulture));
        Replace(identity, IamClaimTypes.ChainId, session.ChainId?.ToString(CultureInfo.InvariantCulture));
    }

    private static void Replace(ClaimsIdentity identity, string claimType, string? value)
    {
        foreach (var claim in identity.FindAll(claimType).ToList())
            identity.RemoveClaim(claim);
        if (!string.IsNullOrEmpty(value))
            identity.AddClaim(new Claim(claimType, value));
    }

    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        // Replace the default empty 401 with a ProblemDetails body (the WWW-Authenticate header is kept).
        context.HandleResponse();

        var detail = context.AuthenticateFailure switch
        {
            null => MissingTokenDetail,
            SecurityTokenExpiredException => ExpiredTokenDetail,
            SecurityTokenException => InvalidTokenDetail,
            { Message: var message } when !string.IsNullOrWhiteSpace(message) => message,
            _ => InvalidTokenDetail
        };

        var response = context.Response;
        response.StatusCode = StatusCodes.Status401Unauthorized;
        response.Headers.WWWAuthenticate = context.AuthenticateFailure is null
            ? "Bearer"
            : "Bearer error=\"invalid_token\"";

        await WriteProblemAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized", detail);
    }

    public override Task Forbidden(ForbiddenContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return WriteProblemAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Forbidden", ForbiddenDetail);
    }

    private async Task WriteProblemAsync(HttpContext httpContext, int status, string title, string detail)
    {
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }
}
