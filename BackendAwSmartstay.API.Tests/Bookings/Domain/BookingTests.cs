using System;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using FluentAssertions;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Bookings.Domain;

public class BookingTests
{
    [Fact]
    public void Create_WithoutGuestProfileId_ShouldInitializeWithNullGuestProfileId_AndPendingStatus()
    {
        // Arrange
        const int roomId = 101;
        const string guestName = "Alice Wonderland";
        const string guestEmail = "alice@example.com";
        var checkIn = DateTime.UtcNow.AddDays(1);
        var checkOut = DateTime.UtcNow.AddDays(5);

        // Act
        var booking = new Booking(roomId, guestName, guestEmail, checkIn, checkOut);

        // Assert
        booking.RoomId.Should().Be(roomId);
        booking.GuestName.Should().Be(guestName);
        booking.GuestEmail.Should().Be(guestEmail);
        booking.CheckInDate.Should().Be(checkIn);
        booking.CheckOutDate.Should().Be(checkOut);
        booking.GuestProfileId.Should().BeNull();
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public void Create_WithGuestProfileId_ShouldRetainGuestProfileId()
    {
        // Arrange
        const int roomId = 202;
        const string guestName = "Bob Builder";
        const string guestEmail = "bob@example.com";
        var checkIn = DateTime.UtcNow.AddDays(2);
        var checkOut = DateTime.UtcNow.AddDays(4);
        var expectedProfileId = Guid.NewGuid();

        // Act
        var booking = new Booking(roomId, guestName, guestEmail, checkIn, checkOut, expectedProfileId);

        // Assert
        booking.GuestProfileId.Should().Be(expectedProfileId);
        booking.GuestName.Should().Be(guestName);
        booking.GuestEmail.Should().Be(guestEmail);
    }

    [Fact]
    public void Create_FromCommand_ShouldInitializeCorrectly()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            303,
            "Charlie Chaplin",
            "charlie@example.com",
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(7),
            UserId: 42,
            GuestProfileId: profileId);

        // Act
        var booking = new Booking(command);

        // Assert
        booking.RoomId.Should().Be(303);
        booking.GuestName.Should().Be("Charlie Chaplin");
        booking.GuestEmail.Should().Be("charlie@example.com");
        booking.GuestProfileId.Should().Be(profileId);
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public void Confirm_ShouldTransitionStatusToConfirmed()
    {
        // Arrange
        var booking = new Booking(101, "Test User", "test@example.com", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        // Act
        booking.Confirm();

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Cancel_ShouldTransitionStatusToCancelled()
    {
        // Arrange
        var booking = new Booking(101, "Test User", "test@example.com", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        // Act
        booking.Cancel();

        // Assert
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
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
