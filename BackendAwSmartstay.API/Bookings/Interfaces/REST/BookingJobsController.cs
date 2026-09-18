using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST;

/// <summary>
///     Scheduled jobs of the Bookings context, called by the external scheduler with <c>X-Cron-Key</c>. Kept apart
///     from <see cref="BookingsController"/>, whose user-token authorization would otherwise be combined with the
///     scheduler scheme.
/// </summary>
[ApiController]
[Route("api/v1/bookings")]
[Produces("application/json")]
[Tags("Bookings")]
public class BookingJobsController(IBookingCommandService bookingCommandService) : ControllerBase
{
    /// <summary>Cancels the Pending bookings whose payment deadline passed (payment hold). Called by the scheduler.</summary>
    /// <remarks>
    ///     Authenticated with <c>X-Cron-Key</c>. Each expired booking frees its room and its guest receives an e-mail.
    ///     Idempotent: a second run finds nothing to expire.
    /// </remarks>
    [HttpPost("expire-pending")]
    [Authorize(Policy = ScheduledJobsAuthenticationExtensions.RunScheduledJobsPolicy)]
    [SwaggerOperation(Summary = "Expire unpaid bookings", OperationId = "ExpireUnpaidBookings")]
    [ProducesResponseType(typeof(ExpiredBookingsResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpireUnpaidBookings()
    {
        var expired = await bookingCommandService.Handle(new ExpireUnpaidBookingsCommand());
        return Ok(new ExpiredBookingsResource(expired));
    }
}
