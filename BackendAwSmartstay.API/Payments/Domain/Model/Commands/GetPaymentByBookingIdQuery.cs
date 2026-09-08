namespace BackendAwSmartstay.API.Payments.Domain.Model.Commands;

/// <summary>
/// Command to initiate a payment processing request.
/// </summary>
/// <param name="BookingId">The identifier of the booking to pay for.</param>
/// <param name="Amount">The total amount to charge.</param>
/// <param name="PaymentMethod">The method used (e.g., "CreditCard").</param>
/// <param name="CardNumber">The raw card number (will be masked).</param>
/// <param name="CardHolderName">Name on the card.</param>
/// <param name="ExpirationDate">Card expiration (MM/YY).</param>
/// <param name="Cvv">Card security code.</param>
public record ProcessPaymentCommand(
    int BookingId, 
    decimal Amount, 
    string PaymentMethod,
    string CardNumber,
    string CardHolderName,
    string ExpirationDate,
    string Cvv
);