using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST;

/// <summary>
///     Scheduled jobs of the rooms, called by the external scheduler with <c>X-Cron-Key</c> (kept apart from
///     <see cref="RoomsController"/>, whose user-token authorization would otherwise be combined with it).
/// </summary>
[ApiController]
[Route("api/v1/rooms")]
[Produces("application/json")]
[Tags("Rooms")]
public class RoomJobsController(IRoomCommandService roomCommandService) : ControllerBase
{
    /// <summary>Alerts the hotel administrators of rooms under maintenance for too long (US-06 scenario 4).</summary>
    /// <remarks>
    ///     Every room in Maintenance for at least <c>Rooms__MaintenanceAlertAfterHours</c> (24 h) whose current
    ///     maintenance period was not alerted yet: its hotel admins (or the chain admins, if the hotel has none) get an
    ///     e-mail. Idempotent: one alert per maintenance period; a new period (the room left Maintenance and came back)
    ///     can be alerted again. Run it hourly.
    /// </remarks>
    [HttpPost("maintenance-alerts")]
    [Authorize(Policy = ScheduledJobsAuthenticationExtensions.RunScheduledJobsPolicy)]
    [SwaggerOperation(Summary = "Alert rooms under maintenance for too long", OperationId = "RaiseMaintenanceAlerts")]
    [ProducesResponseType(typeof(MaintenanceAlertsResultResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> RaiseMaintenanceAlerts()
    {
        var alerted = await roomCommandService.Handle(new RaiseMaintenanceAlertsCommand());
        return Ok(new MaintenanceAlertsResultResource(alerted));
    }
}
