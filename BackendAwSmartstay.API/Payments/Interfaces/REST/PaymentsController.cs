using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Domain.Model.Queries;
using BackendAwSmartstay.API.Payments.Domain.Services;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Payments.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST;

/// <summary>
///     Payments of bookings (D1: paying confirms the booking). The hotel registers the payments it receives (Yape,
///     Plin, transfer, cash, card on its POS); guests can also pay online with a card through a simulated gateway, whose
///     card data is never stored.
/// </summary>
[Authorize]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Payments: payments registered by the hotel that confirm bookings")]
public class PaymentsController(
    IPaymentCommandService paymentCommandService,
    IPaymentQueryService paymentQueryService) : ControllerBase
{
    /// <summary>Registers the payment of a booking and confirms it (US-07 scenario 5).</summary>
    /// <remarks>
    ///     Reception, admin (bookings of their hotel) and chain admin. The amount is always the booking total
    ///     (<c>totalPrice</c>); only a Pending booking can be paid. On success the booking becomes Confirmed in the same
    ///     transaction and the guest receives a confirmation e-mail. <c>operationNumber</c> is required for every method
    ///     except Cash.
    /// </remarks>
    [HttpPost("api/v1/bookings/{bookingId:int}/payments")]
    [Authorize(Policy = Policies.RegisterPayments)]
    [SwaggerOperation(Summary = "Register the payment of a booking", OperationId = "RegisterPayment")]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPayment(int bookingId, [FromBody] RegisterPaymentResource resource)
    {
        var payment = await paymentCommandService.Handle(new RegisterPaymentCommand(bookingId, resource.ToMethod(),
            resource.OperationNumber, resource.Note, User.GetUserId(), User.GetHotelId(), User.IsChainAdmin()));
        return CreatedAtAction(nameof(GetPaymentByBooking), new { bookingId = payment.BookingId },
            PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
    }

    /// <summary>The guest pays their own booking online with a card (simulated gateway) and it is confirmed.</summary>
    /// <remarks>
    ///     Guests only, for their own Pending booking (404 otherwise). The amount is always the booking total. No money
    ///     moves and the card is never stored: the payment keeps only the brand and last four digits. A declined test
    ///     card (4000000000000002, 4000000000009995, 4000000000000127) answers 409 <c>payment.card_declined</c> and
    ///     the guest can retry; any other valid card (e.g. 4242424242424242) is approved and the booking becomes
    ///     Confirmed, with the confirmation e-mail.
    /// </remarks>
    [HttpPost("api/v1/bookings/{bookingId:int}/payments/card")]
    [Authorize(Policy = Policies.PayBookingsByCard)]
    [SwaggerOperation(Summary = "Pay a booking online with a card (simulated)", OperationId = "PayBookingWithCard")]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayWithCard(int bookingId, [FromBody] CardPaymentResource resource)
    {
        var payment = await paymentCommandService.Handle(
            new PayBookingWithCardCommand(bookingId, User.GetUserId(), resource.ToCard()));
        return CreatedAtAction(nameof(GetPaymentByBooking), new { bookingId = payment.BookingId },
            PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
    }

    /// <summary>Gets the payment of a booking.</summary>
    /// <remarks>
    ///     The completed (or refunded) payment if any, otherwise the latest attempt. Guests only for their own bookings,
    ///     staff for the bookings of their hotel (404 otherwise).
    /// </remarks>
    [HttpGet("api/v1/payments/booking/{bookingId:int}")]
    [Authorize(Policy = Policies.ReadPayments)]
    [SwaggerOperation(Summary = "Get the payment of a booking", OperationId = "GetPaymentByBooking")]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentByBooking(int bookingId)
    {
        var query = new GetPaymentByBookingIdQuery(bookingId,
            User.IsGuest() ? User.GetUserId() : null,
            User.IsGuest() || User.IsChainAdmin() ? null : User.GetHotelId() ?? 0);
        var payment = await paymentQueryService.Handle(query);
        return payment is null ? NotFound() : Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
    }
}
