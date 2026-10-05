using System;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using FluentAssertions;
using NUnit.Framework;

namespace BackendAwSmartstay.API.Tests.Bookings.Domain;

[TestFixture]
public class BookingTests
{
    private const int HotelId = 1;
    private static readonly DateTime Today = new(2026, 10, 5);
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan PaymentHold = TimeSpan.FromHours(24);

    private static RoomOffer Room(int roomId = 101, decimal price = 150m, string status = "Available",
        bool acceptsBookings = true) =>
        new(roomId, HotelId, 1, "Double", price, "Double room", [], status, "101", acceptsBookings);

    private static BookingRequester Guest(int userId = 7) => BookingRequester.Guest(userId, "guest@example.com");

    private static Booking PlacePending(int roomId = 101, string guestName = "Alice Wonderland", string guestEmail = "alice@example.com",
        DateTime? checkIn = null, DateTime? checkOut = null, Guid? guestProfileId = null, int? userId = 7) =>
        Booking.Place(userId.HasValue ? Guest(userId.Value).WithGuestProfile(guestProfileId) : BookingRequester.HotelStaff(1, HotelId), Room(roomId),
            new DateRange(checkIn ?? Today.AddDays(1), checkOut ?? Today.AddDays(5)),
            new GuestContact(guestName, guestEmail, null), userId, guestProfileId, Today, Now, PaymentHold);

    [Test]
    public void Create_WithoutGuestProfileId_ShouldInitializeWithNullGuestProfileId_AndPendingStatus()
    {
        // Arrange
        const int roomId = 101;
        const string guestName = "Alice Wonderland";
        const string guestEmail = "alice@example.com";
        var checkIn = Today.AddDays(1);
        var checkOut = Today.AddDays(5);

        // Act
        var booking = PlacePending(roomId, guestName, guestEmail, checkIn, checkOut, guestProfileId: null);

        // Assert
        booking.RoomId.Should().Be(roomId);
        booking.GuestName.Should().Be(guestName);
        booking.GuestEmail.Should().Be(guestEmail);
        booking.CheckInDate.Should().Be(checkIn);
        booking.CheckOutDate.Should().Be(checkOut);
        booking.GuestProfileId.Should().BeNull();
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public void Create_WithGuestProfileId_ShouldRetainGuestProfileId()
    {
        // Arrange
        const int roomId = 202;
        const string guestName = "Bob Builder";
        const string guestEmail = "bob@example.com";
        var checkIn = Today.AddDays(2);
        var checkOut = Today.AddDays(4);
        var expectedProfileId = Guid.NewGuid();

        // Act
        var booking = PlacePending(roomId, guestName, guestEmail, checkIn, checkOut, expectedProfileId);

        // Assert
        booking.GuestProfileId.Should().Be(expectedProfileId);
        booking.GuestName.Should().Be(guestName);
        booking.GuestEmail.Should().Be(guestEmail);
    }

    [Test]
    public void Confirm_ShouldTransitionStatusToConfirmed()
    {
        // Arrange
        var booking = PlacePending(101, "Test User", "test@example.com", Today.AddDays(1), Today.AddDays(2));

        // Act
        booking.Confirm(Now);

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Test]
    public void Cancel_ShouldTransitionStatusToCancelled()
    {
        // Arrange
        var booking = PlacePending(101, "Test User", "test@example.com", Today.AddDays(1), Today.AddDays(2));

        // Act
        booking.Cancel(Guest(), Today, Now);

        // Assert
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Test]
    public void BookingAggregate_ShouldNotDependOnProfilesClasses()
    {
        // Reflection check: Verify Booking type has no fields or properties of types in Profiles namespace
        var properties = typeof(Booking).GetProperties();
        foreach (var prop in properties)
        {
            prop.PropertyType.FullName.Should().NotContain("Profiles", "Booking domain model must not leak Profiles types");
            prop.PropertyType.FullName.Should().NotContain("GuestProfile", "Booking must only reference identity Guid?");
        }
    }
}
