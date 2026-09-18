using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>
///     Account and session endpoints (EP-01): registration and e-mail verification (US-01), sign-in with temporary
///     lock and "remember me" (US-02), password recovery (US-04). All of them are anonymous.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Authentication: registration, e-mail verification, sign-in, remembered sessions and password recovery")]
public class AuthenticationController(IAuthenticationCommandService authenticationCommandService) : ControllerBase
{
    private const string RecoveryAcceptedMessage =
        "If an account exists for that e-mail, we have sent it a link to reset the password.";

    private const string VerificationAcceptedMessage =
        "If that e-mail belongs to an unverified account, we have sent it a new verification link.";

    /// <summary>Signs in with e-mail and password (US-02).</summary>
    /// <remarks>
    ///     Wrong e-mail and wrong password get the same 401 (the message never says which one failed). After 5
    ///     consecutive failures the account is locked for 15 minutes (configurable) and the owner gets an e-mail;
    ///     while locked, sign-in answers 401 with a lock message and a <c>lockedUntil</c> member. With
    ///     <c>rememberMe</c> the response also carries a refresh token (US-02 scenario 4).
    /// </remarks>
    [HttpPost("sign-in")]
    [EnableRateLimiting(RateLimitPolicies.Credentials)]
    [SwaggerOperation(Summary = "Sign in", OperationId = "SignIn")]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        var result = await authenticationCommandService.Handle(
            SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource));
        return Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromResult(result));
    }

    /// <summary>Renews a remembered session: returns a new access token and a new refresh token (rotation).</summary>
    /// <remarks>
    ///     The presented refresh token is revoked. Presenting an already used refresh token revokes the whole
    ///     session (reuse detection) and answers 401. A password change or reset, or a deactivation, also ends it.
    /// </remarks>
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refresh a remembered session", OperationId = "RefreshSession")]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenResource resource)
    {
        var result = await authenticationCommandService.Handle(new RefreshSessionCommand(resource.RefreshToken!));
        return Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromResult(result));
    }

    /// <summary>Signs out of a remembered session (its refresh tokens are revoked). Idempotent.</summary>
    /// <remarks>The access token keeps working until it expires (30 minutes by default); clients discard it.</remarks>
    [HttpPost("sign-out")]
    [SwaggerOperation(Summary = "Sign out", OperationId = "SignOut")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignOut([FromBody] RefreshTokenResource resource)
    {
        await authenticationCommandService.Handle(new SignOutCommand(resource.RefreshToken!));
        return NoContent();
    }

    /// <summary>Registers a new account and e-mails a verification link (US-01).</summary>
    /// <remarks>
    ///     Missing or malformed fields answer 400 with one entry per field in <c>errors</c>. An e-mail that is
    ///     already registered answers 409 with <c>detail: "Email already registered"</c> and a
    ///     <c>passwordRecoveryUrl</c> member. The account cannot sign in until the e-mail is verified.
    ///     A bearer token is optional and only needed to assign a non-guest role.
    /// </remarks>
    [HttpPost("sign-up")]
    [EnableRateLimiting(RateLimitPolicies.Credentials)]
    [SwaggerOperation(Summary = "Sign up", OperationId = "SignUp")]
    [ProducesResponseType(typeof(SignUpResultResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        // The authentication handler still runs on anonymous endpoints: a valid token identifies the actor.
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource, User.FindUserId());
        var user = await authenticationCommandService.Handle(command);
        return StatusCode(StatusCodes.Status201Created, new SignUpResultResource(user.Id, user.Email.Value, user.EmailVerified,
            "Account created. We have sent a verification link to your e-mail."));
    }

    /// <summary>Confirms the e-mail with the token of the verification link (US-01).</summary>
    [HttpPost("verify-email")]
    [EnableRateLimiting(RateLimitPolicies.Credentials)]
    [SwaggerOperation(Summary = "Verify the e-mail", OperationId = "VerifyEmail")]
    [ProducesResponseType(typeof(MessageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailResource resource)
    {
        await authenticationCommandService.Handle(new VerifyEmailCommand(resource.Token!));
        return Ok(new MessageResource("Your e-mail has been verified."));
    }

    /// <summary>Sends a new verification link. Always 202: the answer never reveals whether the e-mail exists.</summary>
    [HttpPost("verify-email/resend")]
    [EnableRateLimiting(RateLimitPolicies.Credentials)]
    [SwaggerOperation(Summary = "Resend the verification link", OperationId = "ResendEmailVerification")]
    [ProducesResponseType(typeof(MessageResource), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification([FromBody] AccountEmailResource resource)
    {
        await authenticationCommandService.Handle(new ResendEmailVerificationCommand(resource.Email!));
        return Accepted(new MessageResource(VerificationAcceptedMessage));
    }

    /// <summary>Requests a password reset link (US-04 scenarios 1 and 2).</summary>
    /// <remarks>
    ///     Always 202 with the same message, whether or not the e-mail belongs to an account. The link is single use
    ///     and expires after 30 minutes.
    /// </remarks>
    [HttpPost("password-recovery")]
    [EnableRateLimiting(RateLimitPolicies.Credentials)]
    [SwaggerOperation(Summary = "Request password recovery", OperationId = "RequestPasswordRecovery")]
    [ProducesResponseType(typeof(MessageResource), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordRecovery([FromBody] AccountEmailResource resource)
    {
        await authenticationCommandService.Handle(new RequestPasswordRecoveryCommand(resource.Email!));
        return Accepted(new MessageResource(RecoveryAcceptedMessage));
    }

    /// <summary>Sets a new password with the token of the reset link (US-04 scenarios 3 and 4).</summary>
    /// <remarks>
    ///     An expired link answers 410 ("The password reset link has expired. Request a new one."); an unknown or
    ///     already used link answers 400. On success every session is closed and a confirmation e-mail is sent.
    /// </remarks>
    [HttpPost("password-reset")]
    [EnableRateLimiting(RateLimitPolicies.Credentials)]
    [SwaggerOperation(Summary = "Reset the password", OperationId = "ResetPassword")]
    [ProducesResponseType(typeof(MessageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordResource resource)
    {
        await authenticationCommandService.Handle(new ResetPasswordCommand(resource.Token!, resource.NewPassword!));
        return Ok(new MessageResource("Your password has been updated. Sign in with the new password."));
    }
}
