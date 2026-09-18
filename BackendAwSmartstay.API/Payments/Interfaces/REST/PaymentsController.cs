using BackendAwSmartstay.API.Payments.Domain.Model.Queries;
using BackendAwSmartstay.API.Payments.Domain.Services;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST;

/// <summary>
///     RESTful API interface controller responsible for handling operational, guest, and administrative
///     requests tracking the complete transactional processing of financial payment aggregates within the payment bounded context.
/// </summary>
/// <param name="paymentCommandService">The domain command service used to handle financial state transitions and mutations.</param>
/// <param name="paymentQueryService">The domain query service used to handle payment state extraction and auditing queries.</param>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Available Payment Endpoints")]
public class PaymentsController(
    IPaymentCommandService paymentCommandService,
    IPaymentQueryService paymentQueryService) : ControllerBase
{
    /// <summary>
    ///     Processes and records a new financial payment transaction aggregate root inside the persistence layer.
    /// </summary>
    /// <param name="resource">The incoming input resource payload mapping credit parameters and booking context metrics required for transaction execution.</param>
    /// <returns>A created resource response alongside the structural tracking location parameters of the processed transaction aggregate.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.ProcessPayments)]
    [SwaggerOperation(
        Summary = "Process a new payment transaction",
        Description = "Simulates and records a credit card payment for a Pending/Confirmed booking. The amount is computed by the backend (room price per night × nights); any client amount is ignored. Guests can only pay their own bookings. A successful payment confirms the booking; a declined card returns status 'Failed'.",
        OperationId = "ProcessPayment")]
    [SwaggerResponse(StatusCodes.Status201Created, "The payment transaction aggregate root was successfully validated, processed, and tracked.", typeof(PaymentResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The provided processing resource layout contains invalid fields or business constraint violations.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The booking does not exist or does not belong to the guest.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "The booking is cancelled/completed, already paid, or its room no longer exists.")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentResource resource)
    {
        var guestUserId = User.IsGuest() ? User.GetUserId() : (int?)null;
        var command = ProcessPaymentCommandFromResourceAssembler.ToCommandFromResource(resource, guestUserId);
        var payment = await paymentCommandService.Handle(command);

        if (payment is null) return BadRequest("Could not process the payment transaction aggregate context.");

        var paymentResource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
        
        return CreatedAtAction(nameof(GetPaymentByBooking), new { bookingId = payment.BookingId }, paymentResource);
    }

    /// <summary>
    ///     Retrieves the unique financial payment aggregate partition associated with a specific domain booking aggregate indicator.
    /// </summary>
    /// <param name="bookingId">The unique structural domain identity number of the parent booking target context.</param>
    /// <returns>An asynchronous action result containing the matching financial payment resource representation state, or NotFound.</returns>
    [HttpGet("booking/{bookingId:int}")]
    [Authorize(Policy = Policies.ProcessPayments)]
    [SwaggerOperation(
        Summary = "Get payment ledger properties by booking aggregate identifier",
        Description = "Returns the payment of a booking (the completed one if any, otherwise the latest attempt). Guests only see payments of their own bookings (404 otherwise).",
        OperationId = "GetPaymentByBooking")]
    [SwaggerResponse(StatusCodes.Status200OK, "The payment aggregate associated with the booking context was located and converted successfully.", typeof(PaymentResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Standard users are barred from executing cross-ledger transaction audits.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No payment transaction aggregate matched the supplied booking identifier criteria.")]
    public async Task<IActionResult> GetPaymentByBooking(int bookingId)
    {
        var query = new GetPaymentByBookingIdQuery(bookingId, User.IsGuest() ? User.GetUserId() : null);
        var payment = await paymentQueryService.Handle(query);

        if (payment is null) return NotFound();
        var resource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
        return Ok(resource);

    }
}
