using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Events;
using BackendAwSmartstay.API.Payments.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.UnitTests.Payments.Domain;

[TestFixture]
public class PaymentTests
{
    private const int BookingId = 5;
    private const int StaffId = 3;
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    private static Payment RegisterYape(string? operationNumber = "OP-123", string? note = null) =>
        Payment.Register(BookingId, 450m, PaymentMethod.Yape, operationNumber, note, StaffId, Now);

    private static Payment RegisterCash() =>
        Payment.Register(BookingId, 450m, PaymentMethod.Cash, null, null, StaffId, Now);

    private static void AssignId(Payment payment, int id) =>
        typeof(Payment).GetProperty(nameof(Payment.Id))!.SetValue(payment, id);

    [Test]
    public void Register_WithOperationNumber_CreatesPendingPayment()
    {
        var payment = RegisterYape(" OP-123 ", "  Paid at the desk  ");

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Pending));
            Assert.That(payment.BookingId, Is.EqualTo(BookingId));
            Assert.That(payment.Amount, Is.EqualTo(450m));
            Assert.That(payment.Method, Is.EqualTo(PaymentMethod.Yape));
            Assert.That(payment.OperationNumber, Is.EqualTo("OP-123"));
            Assert.That(payment.Note, Is.EqualTo("Paid at the desk"));
            Assert.That(payment.RecordedByUserId, Is.EqualTo(StaffId));
            Assert.That(payment.PaymentDate, Is.EqualTo(Now.UtcDateTime));
        });
    }

    [Test]
    public void Register_Cash_DiscardsOperationNumber()
    {
        var payment = Payment.Register(BookingId, 450m, PaymentMethod.Cash, "IGNORED", null, StaffId, Now);

        Assert.That(payment.OperationNumber, Is.Null);
    }

    [Test]
    public void Register_BlankNote_IsStoredAsNull()
    {
        var payment = RegisterYape(note: "   ");

        Assert.That(payment.Note, Is.Null);
    }

    [TestCase(PaymentMethod.Yape)]
    [TestCase(PaymentMethod.Plin)]
    [TestCase(PaymentMethod.BankTransfer)]
    [TestCase(PaymentMethod.CardAtFrontDesk)]
    public void Register_NonCashWithoutOperationNumber_Throws(PaymentMethod method)
    {
        var ex = Assert.Throws<InvalidFieldException>(() =>
            Payment.Register(BookingId, 450m, method, "  ", null, StaffId, Now));

        Assert.That(ex!.Violation.Code, Is.EqualTo(PaymentErrorCodes.OperationNumberRequired));
    }

    [Test]
    public void Register_OperationNumberTooLong_Throws()
    {
        var tooLong = new string('9', Payment.MaxOperationNumberLength + 1);

        var ex = Assert.Throws<InvalidFieldException>(() => RegisterYape(tooLong));

        Assert.That(ex!.Violation.Code, Is.EqualTo(PaymentErrorCodes.OperationNumberTooLong));
    }

    [Test]
    public void Register_OperationNumberAtMaxLength_IsAccepted()
    {
        var atLimit = new string('9', Payment.MaxOperationNumberLength);

        Assert.That(RegisterYape(atLimit).OperationNumber, Is.EqualTo(atLimit));
    }

    [Test]
    public void Register_NoteTooLong_Throws()
    {
        var tooLong = new string('x', Payment.MaxNoteLength + 1);

        var ex = Assert.Throws<InvalidFieldException>(() => RegisterYape(note: tooLong));

        Assert.That(ex!.Violation.Code, Is.EqualTo(PaymentErrorCodes.NoteTooLong));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Register_InvalidBookingId_Throws(int bookingId)
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Payment.Register(bookingId, 450m, PaymentMethod.Cash, null, null, StaffId, Now));

        Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.InternalInvariant));
    }

    [TestCase(0)]
    [TestCase(-10)]
    public void Register_NonPositiveAmount_Throws(int amount)
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Payment.Register(BookingId, amount, PaymentMethod.Cash, null, null, StaffId, Now));

        Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.InternalInvariant));
    }

    [Test]
    public void Complete_PendingPayment_BecomesCompletedWithReference()
    {
        var payment = RegisterYape();

        payment.Complete("YAPE-OP-123", Now);

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
            Assert.That(payment.TransactionId, Is.EqualTo("YAPE-OP-123"));
        });
    }

    [Test]
    public void Complete_WithoutGeneratedId_DoesNotRaiseEventYet()
    {
        var payment = RegisterYape();
        payment.Complete("YAPE-OP-123", Now);

        Assert.That(payment.DomainEvents, Is.Empty);
    }

    [Test]
    public void Complete_OnceIdIsGenerated_RaisesPaymentCompletedEventOnlyOnce()
    {
        var payment = RegisterYape();
        payment.Complete("YAPE-OP-123", Now);
        AssignId(payment, 42);

        var events = payment.DomainEvents;
        var completed = events.OfType<PaymentCompletedEvent>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(completed.PaymentId, Is.EqualTo(42));
            Assert.That(completed.BookingId, Is.EqualTo(BookingId));
            Assert.That(completed.Amount, Is.EqualTo(450m));
            Assert.That(completed.Method, Is.EqualTo(PaymentMethod.Yape));
            Assert.That(completed.OccurredOn, Is.EqualTo(Now));
            Assert.That(payment.DomainEvents.OfType<PaymentCompletedEvent>().Count(), Is.EqualTo(1));
        });
    }

    [TestCase(PaymentStatus.Completed)]
    [TestCase(PaymentStatus.Failed)]
    [TestCase(PaymentStatus.Refunded)]
    public void Complete_NonPendingPayment_Throws(PaymentStatus status)
    {
        var payment = InStatus(status);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => payment.Complete("REF", Now));

        Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.CannotComplete));
    }

    [Test]
    public void Fail_PendingPayment_BecomesFailedWithReason()
    {
        var payment = RegisterYape();

        payment.Fail("The card was declined.");

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Failed));
            Assert.That(payment.FailureReason, Is.EqualTo("The card was declined."));
        });
    }

    [TestCase(PaymentStatus.Completed)]
    [TestCase(PaymentStatus.Failed)]
    [TestCase(PaymentStatus.Refunded)]
    public void Fail_NonPendingPayment_Throws(PaymentStatus status)
    {
        var payment = InStatus(status);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => payment.Fail("reason"));

        Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.CannotFail));
    }

    [Test]
    public void Refund_CompletedPayment_BecomesRefundedAndRaisesEvent()
    {
        var payment = InStatus(PaymentStatus.Completed);
        AssignId(payment, 42);
        var refundTime = Now.AddDays(1);

        var refunded = payment.Refund(refundTime);

        Assert.Multiple(() =>
        {
            Assert.That(refunded, Is.True);
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Refunded));
            Assert.That(payment.RefundedAt, Is.EqualTo(refundTime));
            var evt = payment.DomainEvents.OfType<PaymentRefundedEvent>().Single();
            Assert.That(evt.PaymentId, Is.EqualTo(42));
            Assert.That(evt.BookingId, Is.EqualTo(BookingId));
            Assert.That(evt.Amount, Is.EqualTo(450m));
        });
    }

    [TestCase(PaymentStatus.Pending)]
    [TestCase(PaymentStatus.Failed)]
    [TestCase(PaymentStatus.Refunded)]
    public void Refund_NonCompletedPayment_IsNoOp(PaymentStatus status)
    {
        var payment = InStatus(status);
        var refundedAtBefore = payment.RefundedAt;
        var refundEventsBefore = payment.DomainEvents.OfType<PaymentRefundedEvent>().Count();

        var refunded = payment.Refund(Now.AddDays(1));

        Assert.Multiple(() =>
        {
            Assert.That(refunded, Is.False);
            Assert.That(payment.Status, Is.EqualTo(status));
            Assert.That(payment.RefundedAt, Is.EqualTo(refundedAtBefore));
            Assert.That(payment.DomainEvents.OfType<PaymentRefundedEvent>().Count(), Is.EqualTo(refundEventsBefore));
        });
    }

    [Test]
    public void ClearDomainEvents_RemovesRaisedEvents()
    {
        var payment = InStatus(PaymentStatus.Completed);
        payment.Refund(Now);

        payment.ClearDomainEvents();

        Assert.That(payment.DomainEvents, Is.Empty);
    }

    private static Payment InStatus(PaymentStatus status)
    {
        var payment = RegisterCash();
        switch (status)
        {
            case PaymentStatus.Completed:
                payment.Complete("REF", Now);
                break;
            case PaymentStatus.Failed:
                payment.Fail("reason");
                break;
            case PaymentStatus.Refunded:
                payment.Complete("REF", Now);
                payment.Refund(Now);
                break;
        }
        return payment;
    }
}
