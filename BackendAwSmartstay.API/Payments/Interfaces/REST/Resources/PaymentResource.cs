namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

/// <summary>
/// Represents the data returned to the client about a payment.
/// </summary>
/// <param name="Id">The unique identifier of the payment.</param>
/// <param name="BookingId">The identifier of the booking associated with the payment.</param>
/// <param name="TransactionId">The transaction identifier from the payment processor.</param>
/// <param name="Amount">The amount paid.</param>
/// <param name="Status">The status of the payment.</param>
/// <param name="CardNumberMasked">The masked credit card number.</param>
/// <param name="PaymentDate">The date when the payment was processed.</param>
public record PaymentResource(
    int Id,
    int BookingId,
    string TransactionId,
    decimal Amount,
    string Status,
    string CardNumberMasked,
    string PaymentDate
);