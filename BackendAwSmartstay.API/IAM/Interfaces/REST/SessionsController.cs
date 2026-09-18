using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST;

/// <summary>Session management of the signed-in user.</summary>
[Authorize]
[ApiController]
[Route("api/v1/authentication")]
[Produces("application/json")]
[Tags("Authentication")]
public class SessionsController(IMfaCommandService mfaCommandService) : ControllerBase
{
    /// <summary>Signs out of every device: every access token and remembered session of the user stops working.</summary>
    /// <remarks>The token used for this call is revoked too; the client must discard it and sign in again.</remarks>
    [HttpPost("sign-out-all")]
    [SwaggerOperation(Summary = "Sign out of all devices", OperationId = "SignOutEverywhere")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOutEverywhere()
    {
        await mfaCommandService.Handle(new SignOutEverywhereCommand(User.GetUserId()));
        return NoContent();
    }
}
