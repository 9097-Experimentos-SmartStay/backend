using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST;

/// <summary>
///     Digital check-in of a booking (US-08). <b>Not part of the current backlog</b>: kept in the API (like the IoT
///     emulator) but no client uses it yet.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/bookings/{bookingId:int}/check-in")]
[Produces("application/json")]
[Tags("Check-in (not part of the current backlog)")]
public class CheckInController(ICheckInService checkInService) : ControllerBase
{
    /// <summary>Largest multipart request accepted (the document itself is limited to 5 MB).</summary>
    private const long MaxRequestBytes = 6 * 1024 * 1024;

    /// <summary>Completes the digital check-in with the identity document (US-08 scenarios 1, 2 and 4).</summary>
    /// <remarks>
    ///     <c>multipart/form-data</c> with <c>documentType</c> (DNI, PASSPORT, CE), <c>documentNumber</c>,
    ///     <c>nationality</c> (ISO alpha-2) and <c>document</c> (JPG, PNG or PDF, ≤ 5 MB, checked by content).
    ///     Only the guest of a <b>Confirmed</b> (paid) booking, from the check-in day until the day before the
    ///     check-out (hotel time). The document format is validated automatically; when it passes, the check-in is
    ///     approved at once: the booking becomes <c>CheckedIn</c>, the room <c>Occupied</c>, the response carries the
    ///     6-digit room <c>accessCode</c> (valid until the check-out) and the hotel's housekeeping is notified by
    ///     e-mail. Errors: 400 per field (<c>documentNumber</c>, <c>nationality</c>, <c>documentType</c>,
    ///     <c>document</c>); 404 not the guest's booking; 409 not confirmed, outside the window, already checked in, or
    ///     the room is not ready (then request assistance).
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = Policies.CompleteCheckIn)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [SwaggerOperation(Summary = "Complete the digital check-in", OperationId = "CompleteCheckIn")]
    [ProducesResponseType(typeof(CheckInResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteCheckIn(int bookingId, [FromForm] CheckInFormResource form)
    {
        using var buffer = new MemoryStream();
        await form.Document!.CopyToAsync(buffer);
        var result = await checkInService.Handle(new CompleteDigitalCheckInCommand(bookingId, User.ToBookingRequester(),
            form.ToDocumentType(), form.DocumentNumber!, form.Nationality!, buffer.ToArray()));
        return CreatedAtAction(nameof(GetCheckIn), new { bookingId }, ToResource(result));
    }

    /// <summary>Gets the check-in of a booking (the access code only for its guest).</summary>
    [HttpGet]
    [Authorize(Policy = Policies.ReadBookings)]
    [SwaggerOperation(Summary = "Get the check-in of a booking", OperationId = "GetCheckIn")]
    [ProducesResponseType(typeof(CheckInResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCheckIn(int bookingId)
    {
        var result = await checkInService.FindAsync(bookingId, User.ToBookingRequester());
        return result is null ? NotFound() : Ok(ToResource(result));
    }

    /// <summary>Asks the front desk for help with the check-in (US-08 scenario 3).</summary>
    /// <remarks>
    ///     The reception staff of the hotel (its admins if there is no reception user) receive an e-mail with the
    ///     booking, the guest's contact and the optional message. Only the guest of a Pending or Confirmed booking.
    /// </remarks>
    [HttpPost("assistance")]
    [Authorize(Policy = Policies.CompleteCheckIn)]
    [SwaggerOperation(Summary = "Request help with the check-in", OperationId = "RequestCheckInAssistance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestAssistance(int bookingId, [FromBody] CheckInAssistanceResource? resource)
    {
        await checkInService.Handle(new RequestCheckInAssistanceCommand(bookingId, User.ToBookingRequester(), resource?.Message));
        return Accepted(new { message = "The front desk has been notified and will contact you shortly." });
    }

    private static CheckInResource ToResource(CheckInResult result) => new(
        result.Booking.Id, result.Booking.Code.Value, result.Booking.Status.ToString(), result.Booking.RoomId,
        result.CheckIn.Status.ToString(),
        result.CheckIn.DocumentType switch
        {
            IdentityDocumentType.Dni => "DNI",
            IdentityDocumentType.Passport => "PASSPORT",
            _ => "CE"
        },
        result.CheckIn.Identity.MaskedNumber, result.CheckIn.Nationality, result.AccessCode?.Value,
        result.CheckIn.AccessCodeValidUntil, result.CheckIn.CompletedAt);
}
