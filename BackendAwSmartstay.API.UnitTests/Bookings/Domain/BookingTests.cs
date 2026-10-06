using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.UnitTests.Bookings.Domain;

[TestFixture]
public class BookingTests
{
    private const int HotelId = 1;
    private static readonly DateTime Today = new(2026, 10, 5);
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan PaymentHold = TimeSpan.FromHours(24);

    private static RoomOffer Room(int roomId = 10, decimal price = 150m, string status = "Available",
        bool acceptsBookings = true) =>
        new(roomId, HotelId, 1, "Double", price, "Double room", [], status, "101", acceptsBookings);

    private static BookingRequester Guest() => BookingRequester.Guest(7, "guest@example.com");

    private static Booking PlacePending(RoomOffer? room = null) =>
        Booking.Place(Guest(), room ?? Room(), new DateRange(Today.AddDays(2), Today.AddDays(5)),
            new GuestContact("Ana Pérez", "ana@example.com", null), null, null, Today, Now, PaymentHold);

    [Test]
    public void Place_ByGuest_CreatesPendingBookingWithPaymentDeadlineAndTotal()
    {
        var booking = PlacePending();

        Assert.Multiple(() =>
        {
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Pending));
            Assert.That(booking.PaymentDueAt, Is.EqualTo(Now + PaymentHold));
            Assert.That(booking.Nights, Is.EqualTo(3));
            Assert.That(booking.TotalPrice, Is.EqualTo(450m));
            Assert.That(booking.GuestId!.Value, Is.EqualTo(7));
        });
    }

    [Test]
    public void Place_RoomUnderMaintenance_Throws()
    {
        Assert.Throws<RoomUnderMaintenanceException>(() => PlacePending(Room(status: "Maintenance")));
    }

    [Test]
    public void Place_HotelWithoutPaymentMethods_Throws()
    {
        Assert.Throws<HotelNotAcceptingBookingsException>(() => PlacePending(Room(acceptsBookings: false)));
    }

    [Test]
    public void Place_ByStaffOfAnotherHotel_Throws()
    {
        var staff = BookingRequester.HotelStaff(99, hotelId: 2);

        Assert.Throws<BookingOutsideHotelScopeException>(() =>
            Booking.Place(staff, Room(), new DateRange(Today.AddDays(1), Today.AddDays(2)),
                new GuestContact("Ana Pérez", "ana@example.com", null), null, null, Today, Now, PaymentHold));
    }

    [Test]
    public void Confirm_PendingBooking_BecomesConfirmedAndClearsPaymentDeadline()
    {
        var booking = PlacePending();

        booking.Confirm(Now);

        Assert.Multiple(() =>
        {
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Confirmed));
            Assert.That(booking.ConfirmedAt, Is.EqualTo(Now));
            Assert.That(booking.PaymentDueAt, Is.Null);
        });
    }

    [Test]
    public void Cancel_ByOwnerBeforeCheckInDay_CancelsWithGuestRequestReason()
    {
        var booking = PlacePending();

        booking.Cancel(Guest(), Today, Now);

        Assert.Multiple(() =>
        {
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
            Assert.That(booking.CancellationReason, Is.EqualTo(CancellationReason.GuestRequest));
        });
    }

    [Test]
    public void Cancel_OnCheckInDay_ThrowsCancellationTooLate()
    {
        var booking = PlacePending();

        var ex = Assert.Throws<InvalidBookingTransitionException>(() =>
            booking.Cancel(Guest(), Today.AddDays(2), Now));

        Assert.That(ex!.Code, Is.EqualTo(BookingErrorCodes.CancellationTooLate));
    }

    [Test]
    public void ExpireIfUnpaid_AfterPaymentDeadline_CancelsBooking()
    {
        var booking = PlacePending();

        var expired = booking.ExpireIfUnpaid(Now + PaymentHold);

        Assert.Multiple(() =>
        {
            Assert.That(expired, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
            Assert.That(booking.CancellationReason, Is.EqualTo(CancellationReason.PaymentNotReceived));
        });
    }

    [Test]
    public void DateRange_BackToBackStays_DoNotOverlap()
    {
        var first = new DateRange(Today, Today.AddDays(3));
        var second = new DateRange(Today.AddDays(3), Today.AddDays(5));

        Assert.That(first.Overlaps(second), Is.False);
    }

    [Test]
    public void DateRange_CheckOutSameDayAsCheckIn_Throws()
    {
        var ex = Assert.Throws<DomainValidationException>(() => new DateRange(Today, Today));

        Assert.That(ex!.Code, Is.EqualTo(BookingErrorCodes.CheckOutNotAfterCheckIn));
    }
}
