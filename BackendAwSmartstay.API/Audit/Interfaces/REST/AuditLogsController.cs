using BackendAwSmartstay.API.Audit.Domain.Model.Queries;
using BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Audit.Domain.Services;
using BackendAwSmartstay.API.Audit.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Audit.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Audit.Interfaces.REST;

/// <summary>
///     Access audit log (US-03 scenario 4): sign-ins, failed sign-ins, locks, sign-outs, password resets and
///     changes, and user administration (creation, role changes, deactivation and activation).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize(Policy = Policies.ViewAuditLog)]
[SwaggerTag("Audit log: who did what and when (administrators)")]
public class AuditLogsController(IAuditQueryService auditQueryService) : ControllerBase
{
    /// <summary>Lists the audit log, newest first.</summary>
    /// <remarks>
    ///     A chain_admin sees every entry; an admin only the entries about accounts of their hotel.
    ///     <c>userId</c> matches the actor or the target of the action.
    /// </remarks>
    /// <param name="userId">Only entries where this user acted or was acted upon.</param>
    /// <param name="action">Only this action (e.g. SignInFailed).</param>
    /// <param name="from">From this moment (ISO 8601, inclusive).</param>
    /// <param name="to">Until this moment (ISO 8601, inclusive).</param>
    /// <param name="page">Page number, from 1.</param>
    /// <param name="pageSize">Items per page, 1 to 100.</param>
    [HttpGet]
    [SwaggerOperation(Summary = "List the audit log", OperationId = "GetAuditLogs")]
    [ProducesResponseType(typeof(PagedResource<AuditEntryResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int? userId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var scope = User.IsChainAdmin() ? AuditReadScope.Everything() : AuditReadScope.OfHotel(User.GetHotelId());
        var result = await auditQueryService.Handle(new GetAuditEntriesQuery(scope, userId, action, from, to, page, pageSize));
        return Ok(AuditEntryResourceFromEntityAssembler.ToResourceFromPage(result));
    }
}
