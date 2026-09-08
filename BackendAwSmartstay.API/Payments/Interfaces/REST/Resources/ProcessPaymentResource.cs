using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

/// <summary>
/// Represents the data required to process a payment.
/// </summary>
/// <param name="BookingId">The identifier of the booking for which the payment is being processed.</param>
/// <param name="Amount">The amount to be paid.</param>
/// <param name="PaymentMethod">The method of payment (e.g., credit card).</param>
/// <param name="CardNumber">The credit card number.</param>
/// <param name="CardHolderName">The name of the card holder.</param>
/// <param name="ExpirationDate">The expiration date of the card.</param>
/// <param name="Cvv">The CVV code of the card.</param>
public record ProcessPaymentResource(
    [Required] int BookingId,
    [Required] decimal Amount,
    [Required] string PaymentMethod,
    [Required] [CreditCard] string CardNumber,
    [Required] string CardHolderName,
    [Required] string ExpirationDate,
    [Required] string Cvv
);