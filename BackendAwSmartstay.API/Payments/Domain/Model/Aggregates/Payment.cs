using BackendAwSmartstay.API.Payments.Domain.Model.Events;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;

/// <summary>
///     The payment of a booking (D1: paying confirms the booking). The amount is always the booking's total, computed
///     by the backend; the client never sends it. Card data is never received nor stored.
/// </summary>
public class Payment : IHasDomainEvents
{
    public const int MaxOperationNumberLength = 50;
    public const int MaxNoteLength = 300;

    private readonly List<IEvent> _domainEvents = [];

    /// <summary>EF Core constructor.</summary>
    protected Payment()
    {
        TransactionId = string.Empty;
    }

    public int Id { get; private set; }

    /// <summary>The booking this payment is for.</summary>
    public int BookingId { get; private set; }

    /// <summary>Reference given by the payment gateway (for manual payments, derived from the operation number).</summary>
    public string TransactionId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    /// <summary>Operation number of Yape, Plin, the transfer or the POS voucher (none for cash).</summary>
    public string? OperationNumber { get; private set; }

    public string? Note { get; private set; }

    /// <summary>The staff member who registered it.</summary>
    public int? RecordedByUserId { get; private set; }

    /// <summary>When it was registered (UTC).</summary>
    public DateTime PaymentDate { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset? RefundedAt { get; private set; }

    public IReadOnlyCollection<IEvent> DomainEvents
    {
        get
        {
            // PaymentCompletedEvent carries the generated id: it is recorded once the payment has it.
            if (_completedAt is { } at && Id > 0)
            {
                _domainEvents.Insert(0, new PaymentCompletedEvent(Id, BookingId, Amount, Method, at));
                _completedAt = null;
            }
            return _domainEvents.AsReadOnly();
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    ///     A payment received by the hotel for a booking (US-07 scenario 5). Every method except cash needs the
    ///     operation number that proves it.
    /// </summary>
    public static Payment Register(int bookingId, decimal amount, PaymentMethod method, string? operationNumber,
        string? note, int recordedByUserId, DateTimeOffset now)
    {
        if (bookingId <= 0) throw new DomainValidationException("A payment must reference a booking.");
        if (amount <= 0) throw new DomainValidationException("The amount to pay must be positive.");

        var operation = string.IsNullOrWhiteSpace(operationNumber) ? null : operationNumber.Trim();
        if (method != PaymentMethod.Cash && operation is null)
            throw new InvalidFieldException("operationNumber", $"Enter the operation number of the {method} payment.");
        if (operation is { Length: > MaxOperationNumberLength })
            throw new InvalidFieldException("operationNumber", $"The operation number cannot exceed {MaxOperationNumberLength} characters.");
        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (trimmedNote is { Length: > MaxNoteLength })
            throw new InvalidFieldException("note", $"The note cannot exceed {MaxNoteLength} characters.");

        return new Payment
        {
            BookingId = bookingId,
            Amount = amount,
            Method = method,
            OperationNumber = method == PaymentMethod.Cash ? null : operation,
            Note = trimmedNote,
            RecordedByUserId = recordedByUserId,
            PaymentDate = now.UtcDateTime,
            Status = PaymentStatus.Pending
        };
    }

    /// <summary>The gateway approved it. Raises <see cref="PaymentCompletedEvent"/> once the payment has its id.</summary>
    public void Complete(string transactionReference, DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending)
            throw new BusinessRuleViolationException($"A {Status.ToString().ToLowerInvariant()} payment cannot be completed.");
        TransactionId = transactionReference;
        Status = PaymentStatus.Completed;
        _completedAt = now;
    }

    /// <summary>The gateway rejected it.</summary>
    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new BusinessRuleViolationException($"A {Status.ToString().ToLowerInvariant()} payment cannot fail.");
        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }

    /// <summary>
    ///     R2: the booking of a completed payment was cancelled. The money is returned outside the system (e.g. a Yape
    ///     transfer back); the payment stops counting as revenue. No-op for any other status.
    /// </summary>
    public bool Refund(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Completed) return false;
        Status = PaymentStatus.Refunded;
        RefundedAt = now;
        _domainEvents.Add(new PaymentRefundedEvent(Id, BookingId, Amount, now));
        return true;
    }

    private DateTimeOffset? _completedAt;
}

/// <summary>
/// Enumeration of possible payment statuses.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}
