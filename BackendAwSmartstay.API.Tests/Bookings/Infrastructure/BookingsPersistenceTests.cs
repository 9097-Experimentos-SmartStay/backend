using System;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BackendAwSmartstay.API.Tests.Bookings.Infrastructure;

[TestFixture]
public class BookingsPersistenceTests
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
            new DateRange(checkIn ?? Today.AddDays(1), checkOut ?? Today.AddDays(4)),
            new GuestContact(guestName, guestEmail, null), userId, guestProfileId, Today, Now, PaymentHold);

    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
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
            var booking = PlacePending(
                roomId: 101,
                guestName: "Alice Wonderland",
                guestEmail: "alice@example.com",
                checkIn: Today.AddDays(1),
                checkOut: Today.AddDays(4),
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

    [Test]
    public async Task Booking_WithoutGuestProfileId_ShouldPersist_AndMaterializeWithNullGuestProfileId()
    {
        // Arrange
        var options = CreateNewContextOptions();
        int bookingId;

        // 1. Persist
        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var booking = PlacePending(
                roomId: 202,
                guestName: "Anonymous Guest",
                guestEmail: "anon@example.com",
                checkIn: Today.AddDays(2),
                checkOut: Today.AddDays(5),
                guestProfileId: null,
                userId: null);

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

    [Test]
    public async Task Booking_StatusTransition_ShouldPersistCorrectly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        int bookingId;

        using (var context = new AppDbContext(options))
        {
            var repo = new BookingRepository(context);
            var booking = PlacePending(
                roomId: 10,
                guestName: "Bob Tester",
                guestEmail: "bob@example.com",
                checkIn: Today.AddDays(1),
                checkOut: Today.AddDays(2));

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
            booking!.Confirm(Now);
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
