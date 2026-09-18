using System.Globalization;
using System.Security.Claims;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authentication;
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
///         <item>writes 401/403 responses as RFC 7807 ProblemDetails through the native problem details service,
///         with the stable code of the rejection (<see cref="BearerRejection"/>).</item>
///     </list>
/// </summary>
public class IamJwtBearerEvents(
    IUserQueryService userQueryService,
    IProblemDetailsService problemDetailsService,
    ILogger<IamJwtBearerEvents> logger) : JwtBearerEvents
{

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal!;
        var userId = principal.FindUserId();
        var tokenVersion = principal.GetTokenVersion();

        if (userId is null || tokenVersion is null)
        {
            context.Fail(new BearerTokenRejectedException(BearerRejection.Invalid));
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
                context.Fail(new BearerTokenRejectedException(BearerRejection.Deactivated));
                return;
            case UserSessionStatus.UserNotFound:
                logger.LogWarning("Rejected token of unknown user {UserId}.", userId);
                context.Fail(new BearerTokenRejectedException(BearerRejection.Revoked));
                return;
            default:
                logger.LogInformation("Rejected revoked token (version {TokenVersion}) of user {UserId}.", tokenVersion, userId);
                context.Fail(new BearerTokenRejectedException(BearerRejection.Revoked));
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

        // Only our own session rejections (TokenValidated -> Fail) are described; library errors are never echoed.
        var rejection = context.AuthenticateFailure switch
        {
            null => BearerRejection.Missing,
            SecurityTokenExpiredException => BearerRejection.Expired,
            BearerTokenRejectedException rejected => rejected.Rejection,
            _ => BearerRejection.Invalid
        };

        var response = context.Response;
        response.StatusCode = StatusCodes.Status401Unauthorized;
        response.Headers.WWWAuthenticate = context.AuthenticateFailure is null
            ? "Bearer"
            : "Bearer error=\"invalid_token\"";

        await WriteProblemAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized", rejection);
    }

    public override Task Forbidden(ForbiddenContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return WriteProblemAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Forbidden", BearerRejection.Forbidden);
    }

    private async Task WriteProblemAsync(HttpContext httpContext, int status, string title, BearerRejection rejection)
    {
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = rejection.Detail,
                Extensions = { [ProblemCodes.CodeExtension] = rejection.Code }
            }
        });
    }
}
