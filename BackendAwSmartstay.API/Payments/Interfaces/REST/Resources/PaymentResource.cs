namespace BackendAwSmartstay.API.Payments.Interfaces.REST.Resources;

/// <summary>The payment of a booking.</summary>
/// <param name="Id">Payment id.</param>
/// <param name="BookingId">The booking paid.</param>
/// <param name="TransactionId">Reference of the gateway (manual payments: method + operation number).</param>
/// <param name="Amount">Amount: the booking total (computed by the backend).</param>
/// <param name="Status">Completed, Failed or Refunded.</param>
/// <param name="Method">Yape, Plin, BankTransfer, Cash or CardAtFrontDesk.</param>
/// <param name="OperationNumber">Operation number (null for cash).</param>
/// <param name="Note">Note of the staff member.</param>
/// <param name="RecordedByUserId">Staff member who registered it.</param>
/// <param name="PaymentDate">When it was registered, "yyyy-MM-dd HH:mm:ss" UTC.</param>
/// <param name="RefundedAt">When it was marked refunded (cancelled paid booking).</param>
public record PaymentResource(
    int Id,
    int BookingId,
    string TransactionId,
    decimal Amount,
    string Status,
    string Method,
    string? OperationNumber,
    string? Note,
    int? RecordedByUserId,
    string PaymentDate,
    DateTimeOffset? RefundedAt);
