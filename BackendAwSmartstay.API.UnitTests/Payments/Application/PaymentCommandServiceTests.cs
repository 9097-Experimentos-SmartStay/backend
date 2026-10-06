using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Commands;
using BackendAwSmartstay.API.Payments.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendAwSmartstay.API.UnitTests.Payments.Application;

[TestFixture]
public class PaymentCommandServiceTests
{
    private const int BookingId = 5;
    private const int HotelId = 1;
    private const int GuestId = 7;
    private const int StaffId = 3;
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    private FakePaymentRepository _payments = null!;
    private FakeBookingsFacade _bookings = null!;
    private FakeUnitOfWork _unitOfWork = null!;
    private StubPaymentGateway _gateway = null!;
    private StubCardPaymentGateway _cardGateway = null!;

    [SetUp]
    public void SetUp()
    {
        _payments = new FakePaymentRepository();
        _bookings = new FakeBookingsFacade { Booking = Booking(), OwnerGuestId = GuestId };
        _unitOfWork = new FakeUnitOfWork();
        _gateway = new StubPaymentGateway(PaymentGatewayResult.Approve("YAPE-OP-123"));
        _cardGateway = new StubCardPaymentGateway(PaymentGatewayResult.Approve("SIM-VISA-4242-ABC"));
    }

    private PaymentCommandService Service() =>
        new(_payments, _bookings, _gateway, _cardGateway, _unitOfWork, new FixedTimeProvider(Now),
            NullLogger<PaymentCommandService>.Instance);

    private static BookingSnapshot Booking(string status = "Pending", bool canBePaid = true, int hotelId = HotelId) =>
        new(BookingId, 10, new DateTime(2026, 10, 7), new DateTime(2026, 10, 10), 3, status, canBePaid, hotelId,
            "BK-0005", 450m, "guest@example.com");

    private static RegisterPaymentCommand Register(PaymentMethod method = PaymentMethod.Yape,
        string? operationNumber = "OP-123", int? staffHotelId = HotelId, bool allHotels = false) =>
        new(BookingId, method, operationNumber, null, StaffId, staffHotelId, allHotels);

    private static PayBookingWithCardCommand PayWithCard(int guestId = GuestId) =>
        new(BookingId, guestId, new CardDetails("4242424242424242", "Ana Pérez", 12, 2030, "123"));

    // ---------- RegisterPaymentCommand ----------

    [Test]
    public async Task RegisterPayment_PendingBooking_CompletesPaymentAndConfirmsBooking()
    {
        var payment = await Service().Handle(Register());

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
            Assert.That(payment.Amount, Is.EqualTo(450m), "the amount always comes from the booking total");
            Assert.That(payment.TransactionId, Is.EqualTo("YAPE-OP-123"));
            Assert.That(payment.RecordedByUserId, Is.EqualTo(StaffId));
            Assert.That(_payments.Payments, Has.Count.EqualTo(1));
            Assert.That(_bookings.ConfirmedBookingIds, Is.EqualTo(new[] { BookingId }));
            Assert.That(_unitOfWork.TransactionCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task RegisterPayment_SendsTheBookingChargeToTheGateway()
    {
        await Service().Handle(Register(PaymentMethod.Plin, "PL-9"));

        Assert.Multiple(() =>
        {
            Assert.That(_gateway.LastCharge!.BookingId, Is.EqualTo(BookingId));
            Assert.That(_gateway.LastCharge.BookingCode, Is.EqualTo("BK-0005"));
            Assert.That(_gateway.LastCharge.Amount, Is.EqualTo(450m));
            Assert.That(_gateway.LastCharge.Method, Is.EqualTo(PaymentMethod.Plin));
            Assert.That(_gateway.LastCharge.OperationNumber, Is.EqualTo("PL-9"));
        });
    }

    [Test]
    public void RegisterPayment_UnknownBooking_ThrowsNotFound()
    {
        _bookings.Booking = null;

        Assert.ThrowsAsync<EntityNotFoundException>(() => Service().Handle(Register()));
    }

    [Test]
    public void RegisterPayment_StaffOfAnotherHotel_ThrowsOutsideHotelScope()
    {
        var ex = Assert.ThrowsAsync<OperationNotAllowedException>(() =>
            Service().Handle(Register(staffHotelId: 99)));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.OutsideHotelScope));
            Assert.That(_payments.Payments, Is.Empty);
            Assert.That(_bookings.ConfirmedBookingIds, Is.Empty);
        });
    }

    [Test]
    public async Task RegisterPayment_ChainAdminOfAnyHotel_IsAllowed()
    {
        var payment = await Service().Handle(Register(staffHotelId: null, allHotels: true));

        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
    }

    [Test]
    public void RegisterPayment_BookingAlreadyPaid_Throws()
    {
        var paid = Payment.Register(BookingId, 450m, PaymentMethod.Cash, null, null, StaffId, Now);
        paid.Complete("CASH-1", Now);
        _payments.Payments.Add(paid);

        var ex = Assert.ThrowsAsync<BusinessRuleViolationException>(() => Service().Handle(Register()));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.BookingAlreadyPaid));
            Assert.That(_payments.Payments, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void RegisterPayment_BookingNotPayable_ThrowsBookingNotPending()
    {
        _bookings.Booking = Booking(status: "Cancelled", canBePaid: false);

        var ex = Assert.ThrowsAsync<BusinessRuleViolationException>(() => Service().Handle(Register()));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.BookingNotPending));
            Assert.That(ex.Message, Does.Contain("cancelled"));
            Assert.That(_bookings.ConfirmedBookingIds, Is.Empty);
        });
    }

    [Test]
    public void RegisterPayment_MissingOperationNumber_ThrowsAndDoesNotConfirm()
    {
        Assert.ThrowsAsync<InvalidFieldException>(() => Service().Handle(Register(operationNumber: null)));
        Assert.That(_bookings.ConfirmedBookingIds, Is.Empty);
    }

    [Test]
    public async Task RegisterPayment_GatewayRejects_SavesFailedPaymentWithoutConfirmingBooking()
    {
        _gateway = new StubPaymentGateway(PaymentGatewayResult.Reject("Rejected by the bank."));

        var payment = await Service().Handle(Register());

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Failed));
            Assert.That(payment.FailureReason, Is.EqualTo("Rejected by the bank."));
            Assert.That(_payments.Payments, Has.Count.EqualTo(1));
            Assert.That(_unitOfWork.CompleteCalls, Is.EqualTo(1));
            Assert.That(_bookings.ConfirmedBookingIds, Is.Empty);
        });
    }

    // ---------- PayBookingWithCardCommand ----------

    [Test]
    public async Task PayWithCard_OwnPendingBooking_CompletesOnlineCardPaymentAndConfirmsBooking()
    {
        var payment = await Service().Handle(PayWithCard());

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
            Assert.That(payment.Method, Is.EqualTo(PaymentMethod.OnlineCard));
            Assert.That(payment.Amount, Is.EqualTo(450m));
            Assert.That(payment.OperationNumber, Is.EqualTo("Visa ****4242"), "only the masked label is kept");
            Assert.That(payment.TransactionId, Is.EqualTo("SIM-VISA-4242-ABC"));
            Assert.That(payment.RecordedByUserId, Is.EqualTo(GuestId));
            Assert.That(_bookings.LastGuestUserIdRequested, Is.EqualTo(GuestId));
            Assert.That(_bookings.ConfirmedBookingIds, Is.EqualTo(new[] { BookingId }));
        });
    }

    [Test]
    public async Task PayWithCard_PassesTheCardToTheCardGateway()
    {
        var command = PayWithCard();

        await Service().Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(_cardGateway.LastCard, Is.EqualTo(command.Card));
            Assert.That(_cardGateway.LastCharge!.Amount, Is.EqualTo(450m));
            Assert.That(_cardGateway.LastCharge.Method, Is.EqualTo(PaymentMethod.OnlineCard));
        });
    }

    [Test]
    public void PayWithCard_BookingOfAnotherGuest_ThrowsNotFound()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(() => Service().Handle(PayWithCard(guestId: 99)));
        Assert.That(_payments.Payments, Is.Empty);
    }

    [Test]
    public void PayWithCard_BookingAlreadyPaid_Throws()
    {
        var paid = Payment.Register(BookingId, 450m, PaymentMethod.OnlineCard, "Visa ****4242", null, GuestId, Now);
        paid.Complete("SIM-1", Now);
        _payments.Payments.Add(paid);

        var ex = Assert.ThrowsAsync<BusinessRuleViolationException>(() => Service().Handle(PayWithCard()));

        Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.BookingAlreadyPaid));
    }

    [Test]
    public void PayWithCard_BookingNotPayable_ThrowsBookingNotPending()
    {
        _bookings.Booking = Booking(status: "Confirmed", canBePaid: false);

        var ex = Assert.ThrowsAsync<BusinessRuleViolationException>(() => Service().Handle(PayWithCard()));

        Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.BookingNotPending));
    }

    [Test]
    public void PayWithCard_CardDeclined_KeepsFailedAttemptAndThrowsCardDeclined()
    {
        _cardGateway = new StubCardPaymentGateway(PaymentGatewayResult.Reject("The card was declined."));

        var ex = Assert.ThrowsAsync<BusinessRuleViolationException>(() => Service().Handle(PayWithCard()));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(PaymentErrorCodes.CardDeclined));
            Assert.That(ex.Message, Is.EqualTo("The card was declined."));
            Assert.That(_payments.Payments.Single().Status, Is.EqualTo(PaymentStatus.Failed));
            Assert.That(_bookings.ConfirmedBookingIds, Is.Empty);
        });
    }

    [Test]
    public async Task PayWithCard_RetryAfterDeclinedAttempt_IsAllowed()
    {
        var failed = Payment.Register(BookingId, 450m, PaymentMethod.OnlineCard, "Visa ****0002", null, GuestId, Now);
        failed.Fail("The card was declined.");
        _payments.Payments.Add(failed);

        var payment = await Service().Handle(PayWithCard());

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
            Assert.That(_payments.Payments, Has.Count.EqualTo(2));
        });
    }
}
