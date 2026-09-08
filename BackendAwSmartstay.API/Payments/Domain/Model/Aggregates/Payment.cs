using BackendAwSmartstay.API.Payments.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;

/// <summary>
/// Represents a payment transaction within the system.
/// Contains details about the amount, method, status, and associated booking.
/// </summary>
public partial class Payment
{
    public Payment()
    {
        TransactionId = Guid.NewGuid().ToString();
        PaymentMethod = string.Empty;
        CardHolderName = string.Empty;
        CardNumberMasked = string.Empty;
        Status = PaymentStatus.Pending;
        PaymentDate = DateTime.UtcNow;
    }

    public Payment(ProcessPaymentCommand command) : this()
    {
        BookingId = command.BookingId;
        Amount = command.Amount;
        PaymentMethod = command.PaymentMethod; // e.g., "Credit Card"
        CardHolderName = command.CardHolderName;
        // Solo guardamos los últimos 4 dígitos por seguridad (PCI Compliance simulado)
        CardNumberMasked = command.CardNumber.Length > 4 
            ? "**** **** **** " + command.CardNumber.Substring(command.CardNumber.Length - 4) 
            : "****";
    }

    /// <summary>
    /// The unique identifier of the payment.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The identifier of the booking associated with this payment.
    /// </summary>
    public int BookingId { get; private set; }

    /// <summary>
    /// The unique transaction identifier generated for this payment.
    /// </summary>
    public string TransactionId { get; private set; } // UUID único de la transacción

    /// <summary>
    /// The amount of the payment.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// The method used for the payment (e.g., Credit Card).
    /// </summary>
    public string PaymentMethod { get; private set; }

    /// <summary>
    /// The name of the card holder.
    /// </summary>
    public string CardHolderName { get; private set; }

    /// <summary>
    /// The masked credit card number for security.
    /// </summary>
    public string CardNumberMasked { get; private set; }

    /// <summary>
    /// The date when the payment was processed.
    /// </summary>
    public DateTime PaymentDate { get; private set; }

    /// <summary>
    /// The current status of the payment.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Marks the payment as successfully completed.
    /// </summary>
    public void Complete()
    {
        Status = PaymentStatus.Completed;
    }

    /// <summary>
    /// Marks the payment as failed.
    /// </summary>
    public void Fail()
    {
        Status = PaymentStatus.Failed;
    }
}

/// <summary>
/// Enumeration of possible payment statuses.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}