using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.Marketing.Domain.Model.Commands;
using BackendAwSmartstay.API.Marketing.Domain.Model.Queries;
using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Marketing.Domain.Services;
using BackendAwSmartstay.API.Marketing.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Marketing.Interfaces.REST.Transform;
using BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Marketing.Interfaces.REST;

/// <summary>
///     Demo requests from the landing (US-27): submission with immediate confirmation, the sales team's list and
///     the automatic follow-up triggered by the scheduler.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Demo requests: the landing's demo form, the sales list and the automatic follow-up")]
public class DemoRequestsController(
    IDemoRequestCommandService demoRequestCommandService,
    IDemoRequestQueryService demoRequestQueryService) : ControllerBase
{
    /// <summary>Submits the demo form (US-27 scenario 1). Anonymous and rate limited per IP.</summary>
    /// <remarks>
    ///     Answers 201 at once; the visitor receives a confirmation e-mail and the sales team a notification.
    ///     Invalid fields answer 400 with one entry per field in <c>errors</c>.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.PublicForms)]
    [SwaggerOperation(Summary = "Request a demo", OperationId = "CreateDemoRequest")]
    [ProducesResponseType(typeof(DemoRequestCreatedResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateDemoRequest([FromBody] CreateDemoRequestResource resource)
    {
        var request = await demoRequestCommandService.Handle(DemoRequestAssemblers.ToCommand(resource));
        return CreatedAtAction(nameof(GetDemoRequestById), new { id = request.Id },
            new DemoRequestCreatedResource(request.Id, request.Status.ToString(),
                "Demo request received. We have sent you a confirmation e-mail and our team will contact you soon."));
    }

    /// <summary>Lists the demo requests, newest first (sales team: chain_admin).</summary>
    /// <param name="status">Received or FollowedUp.</param>
    /// <param name="page">Page number, from 1.</param>
    /// <param name="pageSize">Items per page, 1 to 100.</param>
    [HttpGet]
    [Authorize(Policy = Policies.ManageDemoRequests)]
    [SwaggerOperation(Summary = "List demo requests", OperationId = "GetDemoRequests")]
    [ProducesResponseType(typeof(DemoRequestPageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDemoRequests(
        [FromQuery] DemoRequestStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await demoRequestQueryService.Handle(new GetDemoRequestsQuery(status, page, pageSize));
        return Ok(DemoRequestAssemblers.ToResource(result));
    }

    /// <summary>Gets one demo request (sales team: chain_admin).</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.ManageDemoRequests)]
    [SwaggerOperation(Summary = "Get a demo request", OperationId = "GetDemoRequestById")]
    [ProducesResponseType(typeof(DemoRequestResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDemoRequestById(int id)
    {
        var request = await demoRequestQueryService.Handle(new GetDemoRequestByIdQuery(id));
        return request is null ? NotFound() : Ok(DemoRequestAssemblers.ToResource(request));
    }

    /// <summary>Sends the automatic follow-up to requests still waiting (US-27 scenario 4). Called by the scheduler.</summary>
    /// <remarks>
    ///     Authenticated with the <c>X-Cron-Key</c> header (not a user token). Every request still <c>Received</c>
    ///     after <c>DemoRequests__FollowUpAfterHours</c> (48 h by default) gets one reminder and becomes
    ///     <c>FollowedUp</c>; running it again sends nothing twice.
    /// </remarks>
    [HttpPost("follow-ups")]
    [Authorize(Policy = ScheduledJobsAuthenticationExtensions.RunScheduledJobsPolicy)]
    [SwaggerOperation(Summary = "Send the automatic follow-ups", OperationId = "SendDemoFollowUps")]
    [ProducesResponseType(typeof(DemoFollowUpResultResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendFollowUps()
    {
        var followedUp = await demoRequestCommandService.Handle(new SendDemoFollowUpsCommand());
        return Ok(new DemoFollowUpResultResource(followedUp));
    }
}
