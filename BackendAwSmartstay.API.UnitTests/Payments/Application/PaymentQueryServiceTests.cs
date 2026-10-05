using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.Queries;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.UnitTests.Payments.Application;

[TestFixture]
public class PaymentQueryServiceTests
{
    private const int BookingId = 5;
    private const int HotelId = 1;
    private const int GuestId = 7;
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    private FakePaymentRepository _payments = null!;
    private FakeBookingsFacade _bookings = null!;

    [SetUp]
    public void SetUp()
    {
        _payments = new FakePaymentRepository();
        _bookings = new FakeBookingsFacade
        {
            OwnerGuestId = GuestId,
            Booking = new BookingSnapshot(BookingId, 10, new DateTime(2026, 10, 7), new DateTime(2026, 10, 10), 3,
                "Confirmed", false, HotelId, "BK-0005", 450m, "guest@example.com")
        };
        var payment = Payment.Register(BookingId, 450m, PaymentMethod.Cash, null, null, 3, Now);
        payment.Complete("CASH-1", Now);
        _payments.Payments.Add(payment);
    }

    private PaymentQueryService Service() => new(_payments, _bookings);

    [Test]
    public async Task Handle_WithoutRequesterScope_ReturnsThePayment()
    {
        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId));

        Assert.That(payment, Is.Not.Null);
    }

    [Test]
    public async Task Handle_GuestOwnerOfTheBooking_ReturnsThePayment()
    {
        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId, GuestUserId: GuestId));

        Assert.That(payment, Is.Not.Null);
    }

    [Test]
    public async Task Handle_GuestWhoDoesNotOwnTheBooking_ReturnsNull()
    {
        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId, GuestUserId: 99));

        Assert.That(payment, Is.Null);
    }

    [Test]
    public async Task Handle_StaffOfTheBookingHotel_ReturnsThePayment()
    {
        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId, StaffHotelId: HotelId));

        Assert.That(payment, Is.Not.Null);
    }

    [Test]
    public async Task Handle_StaffOfAnotherHotel_ReturnsNull()
    {
        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId, StaffHotelId: 99));

        Assert.That(payment, Is.Null);
    }

    [Test]
    public async Task Handle_UnknownBookingForScopedRequester_ReturnsNull()
    {
        _bookings.Booking = null;

        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId, StaffHotelId: HotelId));

        Assert.That(payment, Is.Null);
    }

    [Test]
    public async Task Handle_BookingWithoutPayment_ReturnsNull()
    {
        _payments.Payments.Clear();

        var payment = await Service().Handle(new GetPaymentByBookingIdQuery(BookingId));

        Assert.That(payment, Is.Null);
    }
}
