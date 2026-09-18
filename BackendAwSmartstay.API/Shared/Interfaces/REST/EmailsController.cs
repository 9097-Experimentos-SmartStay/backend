using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;
using BackendAwSmartstay.API.Shared.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Shared.Interfaces.REST;

/// <summary>Outgoing e-mails (transactional outbox): the scheduled job that flushes the pending ones.</summary>
[ApiController]
[Route("api/v1/emails")]
[Produces("application/json")]
[SwaggerTag("E-mails: delivery of the pending outgoing e-mails (scheduled job)")]
public class EmailsController(IEmailDispatcher emailDispatcher) : ControllerBase
{
    /// <summary>Delivers the pending e-mails that are due. Called by the external scheduler (<c>X-Cron-Key</c>).</summary>
    /// <remarks>
    ///     The API also delivers them in the background while it is awake; this job covers the time the host sleeps
    ///     (free plan) and e-mails waiting for a retry. Idempotent: an e-mail already sent is never sent again, and
    ///     concurrent runs split the pending e-mails between them.
    /// </remarks>
    [HttpPost("dispatch")]
    [Authorize(Policy = ScheduledJobsAuthenticationExtensions.RunScheduledJobsPolicy)]
    [SwaggerOperation(Summary = "Deliver the pending e-mails", OperationId = "DispatchEmails")]
    [ProducesResponseType(typeof(EmailDispatchResultResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Dispatch(CancellationToken cancellationToken)
    {
        var report = await emailDispatcher.DispatchDueAsync(cancellationToken);
        return Ok(new EmailDispatchResultResource(report.Sent, report.Retrying, report.Failed));
    }
}
