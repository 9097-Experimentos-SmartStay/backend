using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Custom HTTP pipeline middleware responsible for intercepting incoming requests,
///     validating JSON Web Tokens (JWT), and injecting the authenticated User aggregate
///     into the current HTTP context items for downstream authorization filters.
/// </summary>
/// <remarks>
///     This middleware follows a passive authentication strategy, delegating access rejection 
///     responsibilities to role-based authorization attributes. It utilizes structured logging
///     to audit authentication states without polluting the production environment.
/// </remarks>
/// <param name="next">The next delegate pointer in the ASP.NET Core application pipeline.</param>
/// <param name="logger">The structured logger instance dedicated to tracking identity verification events.</param>
public class RequestAuthorizationMiddleware(
    RequestDelegate next,
    ILogger<RequestAuthorizationMiddleware> logger)
{
    /// <summary>
    ///     Executes the middleware logic for processing the token verification flow.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> of the executing request.</param>
    /// <param name="userQueryService">The domain query service used to retrieve user aggregates.</param>
    /// <param name="tokenService">The outbound service handling token parsing and validation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        logger.LogDebug("Entering RequestAuthorizationMiddleware pipeline invocation.");
        
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            logger.LogDebug("Execution endpoint is null. Bypassing identity extraction pipeline.");
            await next(context);
            return;
        }
        
        // Anonymous endpoints still resolve the caller when a valid token is sent (optional authentication).
        // This is what allows an authenticated admin to call sign-up with a non-guest role.
        // An invalid/expired/revoked token on an anonymous endpoint is simply ignored.
        var allowAnonymous = endpoint.Metadata.Any(metadata => 
            metadata.GetType() == typeof(AllowAnonymousAttribute) ||
            metadata is Microsoft.AspNetCore.Authorization.IAllowAnonymous);

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogDebug("No authorization header token detected. Handing context execution over to authorization filters.");
            await next(context);
            return;
        }

        var userId = await tokenService.ValidateToken(token);

        if (userId == null)
        {
            logger.LogWarning("Identity validation failed: The provided token is malformed, invalid, or expired.");
            await next(context);
            return;
        }

        var getUserByIdQuery = new GetUserByIdQuery(userId.Value);
        var user = await userQueryService.Handle(getUserByIdQuery);

        string? rejectionReason = null;
        if (user == null)
        {
            logger.LogError("Critical Security Inconsistency: Token signature is valid for user ID {UserId}, but the corresponding User aggregate does not exist in the persistence layer.", userId.Value);
            rejectionReason = "Token revocado. Inicie sesión nuevamente.";
        }
        else if (user.Status == UserStatus.Inactive)
        {
            logger.LogWarning("Authentication rejected: User ID {UserId} is inactive.", user.Id);
            rejectionReason = "La cuenta ha sido desactivada. Contacte al administrador.";
        }
        else
        {
            // --- Token version validation ---
            var jwtReader = new JsonWebTokenHandler();
            var jsonWebToken = jwtReader.ReadJsonWebToken(token);
            var tokenVersionClaim = jsonWebToken.Claims.FirstOrDefault(c => c.Type == "token_version");

            if (tokenVersionClaim == null || !int.TryParse(tokenVersionClaim.Value, out var tokenVersion))
            {
                logger.LogWarning("Token is missing 'token_version' claim for user ID {UserId}. Rejecting.", user.Id);
                rejectionReason = "Token revocado. Inicie sesión nuevamente.";
            }
            else if (tokenVersion != user.TokenVersion)
            {
                logger.LogWarning("Token version mismatch for user ID {UserId}: token={TokenVersion}, db={DbVersion}.", user.Id, tokenVersion, user.TokenVersion);
                rejectionReason = "Token revocado. Inicie sesión nuevamente.";
            }
        }

        if (rejectionReason != null)
        {
            if (allowAnonymous)
            {
                await next(context);
                return;
            }

            await WriteUnauthorizedAsync(context, rejectionReason);
            return;
        }

        logger.LogInformation("Successful identity verification for user ID: {UserId} with associated Role: {Role}.", user.Id, user.Role);
        context.Items["User"] = user;

        await next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = detail,
                Instance = context.Request.Path
            },
            options: (JsonSerializerOptions?)null,
            contentType: "application/problem+json");
    }
}