namespace BackendAwSmartstay.API.Payments.Domain.Model.Commands;

/// <summary>
/// Command to initiate a payment processing request.
/// The amount is not part of the command: it is always computed by the backend from the booking.
/// </summary>
/// <param name="BookingId">The identifier of the booking to pay for.</param>
/// <param name="PaymentMethod">The method used (e.g., "CreditCard").</param>
/// <param name="CardNumber">The raw card number (will be masked).</param>
/// <param name="CardHolderName">Name on the card.</param>
/// <param name="ExpirationDate">Card expiration (MM/YY).</param>
/// <param name="Cvv">Card security code.</param>
/// <param name="GuestUserId">When set, the payer is a guest and must own the booking.</param>
public record ProcessPaymentCommand(
    int BookingId, 
    string PaymentMethod,
    string CardNumber,
    string CardHolderName,
    string ExpirationDate,
    string Cvv,
    int? GuestUserId = null
);
