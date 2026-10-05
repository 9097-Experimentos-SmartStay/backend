using BackendAwSmartstay.API.Bookings.Domain.Model.Events;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Payments.Application.Internal.EventHandlers;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.UnitTests.Payments.Application;

[TestFixture]
public class RefundPaymentOnBookingCancelledHandlerTests
{
    private const int BookingId = 5;
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    private FakePaymentRepository _payments = null!;
    private FakeUnitOfWork _unitOfWork = null!;

    [SetUp]
    public void SetUp()
    {
        _payments = new FakePaymentRepository();
        _unitOfWork = new FakeUnitOfWork();
    }

    private RefundPaymentOnBookingCancelledHandler Handler() =>
        new(_payments, _unitOfWork, new FixedTimeProvider(Now.AddDays(1)));

    private static BookingCancelledEvent Cancelled(bool wasPaid) =>
        new(BookingId, "BK-0005", 1, CancellationReason.GuestRequest, wasPaid, Now);

    private Payment AddPayment(PaymentStatus status)
    {
        var payment = Payment.Register(BookingId, 450m, PaymentMethod.Cash, null, null, 3, Now);
        if (status == PaymentStatus.Completed) payment.Complete("CASH-1", Now);
        if (status == PaymentStatus.Failed) payment.Fail("declined");
        _payments.Payments.Add(payment);
        return payment;
    }

    [Test]
    public async Task HandleAsync_PaidBookingCancelled_RefundsPaymentAndSaves()
    {
        var payment = AddPayment(PaymentStatus.Completed);

        await Handler().HandleAsync(Cancelled(wasPaid: true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Refunded));
            Assert.That(payment.RefundedAt, Is.EqualTo(Now.AddDays(1)));
            Assert.That(_unitOfWork.CompleteCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task HandleAsync_UnpaidBookingCancelled_DoesNothing()
    {
        var payment = AddPayment(PaymentStatus.Completed);

        await Handler().HandleAsync(Cancelled(wasPaid: false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
            Assert.That(_unitOfWork.CompleteCalls, Is.Zero);
        });
    }

    [Test]
    public async Task HandleAsync_PaidBookingWithoutCompletedPayment_DoesNothing()
    {
        var failed = AddPayment(PaymentStatus.Failed);

        await Handler().HandleAsync(Cancelled(wasPaid: true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(failed.Status, Is.EqualTo(PaymentStatus.Failed));
            Assert.That(_unitOfWork.CompleteCalls, Is.Zero);
        });
    }

    [Test]
    public async Task HandleAsync_CalledTwice_IsIdempotent()
    {
        var payment = AddPayment(PaymentStatus.Completed);

        await Handler().HandleAsync(Cancelled(wasPaid: true), CancellationToken.None);
        await Handler().HandleAsync(Cancelled(wasPaid: true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Refunded));
            Assert.That(_unitOfWork.CompleteCalls, Is.EqualTo(1));
        });
    }
}
