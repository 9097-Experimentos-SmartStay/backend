using System;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Bookings.Infrastructure;

public class BookingsPersistenceTests
{
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Booking_WithGuestProfileId_ShouldPersist_AndMaterializeCorrectly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        var guestProfileId = Guid.NewGuid();
        int bookingId;

        // 1. Persist
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var booking = new Booking(
                roomId: 101,
                guestName: "Alice Wonderland",
                guestEmail: "alice@example.com",
                checkInDate: DateTime.UtcNow.AddDays(1),
                checkOutDate: DateTime.UtcNow.AddDays(4),
                guestProfileId: guestProfileId);

            await repo.AddAsync(booking);
            await context.SaveChangesAsync();
            bookingId = booking.Id;
        }

        // 2. Query in separate DbContext
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var loadedBooking = await repo.FindByIdAsync(bookingId);

            loadedBooking.Should().NotBeNull();
            loadedBooking!.Id.Should().Be(bookingId);
            loadedBooking.RoomId.Should().Be(101);
            loadedBooking.GuestName.Should().Be("Alice Wonderland");
            loadedBooking.GuestEmail.Should().Be("alice@example.com");
            loadedBooking.Status.Should().Be(BookingStatus.Pending);
            loadedBooking.GuestProfileId.Should().Be(guestProfileId);
        }
    }

    [Fact]
    public async Task Booking_WithoutGuestProfileId_ShouldPersist_AndMaterializeWithNullGuestProfileId()
    {
        // Arrange
        var options = CreateNewContextOptions();
        int bookingId;

        // 1. Persist
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var booking = new Booking(
                roomId: 202,
                guestName: "Anonymous Guest",
                guestEmail: "anon@example.com",
                checkInDate: DateTime.UtcNow.AddDays(2),
                checkOutDate: DateTime.UtcNow.AddDays(5),
                guestProfileId: null);

            await repo.AddAsync(booking);
            await context.SaveChangesAsync();
            bookingId = booking.Id;
        }

        // 2. Query in separate DbContext
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var loadedBooking = await repo.FindByIdAsync(bookingId);

            loadedBooking.Should().NotBeNull();
            loadedBooking!.Id.Should().Be(bookingId);
            loadedBooking.RoomId.Should().Be(202);
            loadedBooking.GuestName.Should().Be("Anonymous Guest");
            loadedBooking.GuestEmail.Should().Be("anon@example.com");
            loadedBooking.Status.Should().Be(BookingStatus.Pending);
            loadedBooking.GuestProfileId.Should().BeNull();
        }
    }

    [Fact]
    public async Task Booking_StatusTransition_ShouldPersistCorrectly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        int bookingId;

        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var booking = new Booking(
                roomId: 10,
                guestName: "Bob Tester",
                guestEmail: "bob@example.com",
                checkInDate: DateTime.UtcNow.AddDays(1),
                checkOutDate: DateTime.UtcNow.AddDays(2));

            await repo.AddAsync(booking);
            await context.SaveChangesAsync();
            bookingId = booking.Id;
        }

        // Confirm booking
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var booking = await repo.FindByIdAsync(bookingId);
            booking.Should().NotBeNull();
            booking!.Confirm();
            repo.Update(booking);
            await context.SaveChangesAsync();
        }

        // Verify confirmed
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var loadedBooking = await repo.FindByIdAsync(bookingId);
            loadedBooking.Should().NotBeNull();
            loadedBooking!.Status.Should().Be(BookingStatus.Confirmed);
        }
    }
}
