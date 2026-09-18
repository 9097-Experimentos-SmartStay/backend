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
///         valid (active user, current token version). The token's claims are never rewritten: a change of role or
///         hotel, a password change or reset, a deactivation, a sign-out everywhere or an MFA reset starts a new
///         session generation, and older tokens get 401 <c>auth.session_revoked</c> with the reason;</item>
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

        // The claims of the token (role, hotel, chain) are its permissions and are never rewritten here: any change
        // of them ends the user's sessions, so only the session generation and the account status are checked.
        var session = await userQueryService.Handle(new GetUserSessionQuery(userId.Value, tokenVersion.Value));
        switch (session.Status)
        {
            case UserSessionStatus.Valid:
                return;
            case UserSessionStatus.Inactive:
                logger.LogInformation("Rejected token of inactive user {UserId}.", userId);
                break;
            case UserSessionStatus.UserNotFound:
                logger.LogWarning("Rejected token of unknown user {UserId}.", userId);
                break;
            default:
                logger.LogInformation("Rejected revoked token (version {TokenVersion}, {Reason}) of user {UserId}.",
                    tokenVersion, session.RevocationReason, userId);
                break;
        }
        context.Fail(new BearerTokenRejectedException(BearerRejection.SessionRevoked(session.RevocationReason)));
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
        var problem = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = rejection.Detail,
                Extensions = { [ProblemCodes.CodeExtension] = rejection.Code }
            }
        };
        if (rejection.Reason is not null) problem.ProblemDetails.Extensions["reason"] = rejection.Reason;
        await problemDetailsService.WriteAsync(problem);
    }
}
