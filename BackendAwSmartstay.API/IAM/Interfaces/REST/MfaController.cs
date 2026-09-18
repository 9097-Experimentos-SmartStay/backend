using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>
///     Two-factor authentication with an authenticator app, TOTP (US-52). Staff accounts must use it; the password
///     step of <c>POST /authentication/sign-in</c> returns an <c>mfaToken</c> that only these endpoints accept, sent as
///     <c>Authorization: Bearer &lt;mfaToken&gt;</c>.
/// </summary>
[ApiController]
[Route("api/v1/authentication/mfa")]
[Produces("application/json")]
[EnableRateLimiting(RateLimitPolicies.Credentials)]
[Tags("Authentication")]
public class MfaController(IMfaCommandService mfaCommandService) : ControllerBase
{
    /// <summary>Starts enrolling an authenticator app: returns the secret and the otpauth URI for the QR code.</summary>
    /// <remarks>
    ///     Needs the <c>mfaToken</c> of a sign-in that answered <c>mfaEnrollmentRequired: true</c>. Calling it again
    ///     replaces the secret (a new QR code). MFA is enabled only after <c>/enrollment/confirm</c>.
    /// </remarks>
    [HttpPost("enrollment")]
    [Authorize(Policy = Policies.EnrollSecondFactor)]
    [SwaggerOperation(Summary = "Start the authenticator enrollment", OperationId = "StartMfaEnrollment")]
    [ProducesResponseType(typeof(MfaEnrollmentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartEnrollment()
    {
        var enrollment = await mfaCommandService.Handle(new StartMfaEnrollmentCommand(User.GetUserId()));
        return Ok(new MfaEnrollmentResource(enrollment.Secret, enrollment.OtpAuthUri, enrollment.Issuer,
            enrollment.AccountName, enrollment.Digits, enrollment.PeriodSeconds, enrollment.Algorithm));
    }

    /// <summary>Confirms the enrollment with a code: enables MFA, returns 10 recovery codes and signs in.</summary>
    /// <remarks>
    ///     The response is the sign-in body (access token, and refresh token if "remember me" was asked at sign-in)
    ///     plus <c>recoveryCodes</c>, shown only this time. A wrong code answers 401 and counts toward the lock.
    /// </remarks>
    [HttpPost("enrollment/confirm")]
    [Authorize(Policy = Policies.EnrollSecondFactor)]
    [SwaggerOperation(Summary = "Confirm the authenticator enrollment", OperationId = "ConfirmMfaEnrollment")]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmEnrollment([FromBody] MfaCodeResource resource)
    {
        var result = await mfaCommandService.Handle(new ConfirmMfaEnrollmentCommand(
            User.GetUserId(), Normalize(resource.Code!), User.RememberMeRequested()));
        return Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromResult(result));
    }

    /// <summary>Completes the sign-in with the second factor: a code of the app or a one-time recovery code.</summary>
    /// <remarks>
    ///     Needs the <c>mfaToken</c> of a sign-in that answered <c>mfaRequired: true</c>. A wrong code, a code already
    ///     used (same 30-second step) or an unknown/used recovery code answers 401 and counts toward the temporary
    ///     lock (5 failures). A recovery code works once.
    /// </remarks>
    [HttpPost("verify")]
    [Authorize(Policy = Policies.VerifySecondFactor)]
    [SwaggerOperation(Summary = "Verify the second factor", OperationId = "VerifyMfa")]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] MfaVerificationResource resource)
    {
        var result = await mfaCommandService.Handle(new VerifyMfaCommand(User.GetUserId(),
            resource.Code is null ? null : Normalize(resource.Code), resource.RecoveryCode, User.RememberMeRequested()));
        return Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromResult(result));
    }

    private static string Normalize(string code) => new(code.Where(char.IsDigit).ToArray());
}
