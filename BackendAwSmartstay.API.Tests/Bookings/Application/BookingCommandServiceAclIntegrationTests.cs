using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace BackendAwSmartstay.API.Tests.Bookings.Application;

[TestFixture]
public class BookingCommandServiceAclIntegrationTests
{
    private const int HotelId = 1;
    private static readonly DateTime Today = new(2026, 10, 5);
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan PaymentHold = TimeSpan.FromHours(24);

    private static RoomOffer SampleRoom(int roomId = 101, decimal price = 150m, string status = "Available",
        bool acceptsBookings = true) =>
        new(roomId, HotelId, 1, "Double", price, "Double room", [], status, "101", acceptsBookings);

    private static BookingRequester Guest(int userId = 7, string email = "guest@example.com") =>
        BookingRequester.Guest(userId, email);

    private static Booking PlacePendingBooking(int roomId = 101, string guestName = "Alice Wonderland",
        string guestEmail = "alice@example.com", DateTime? checkIn = null, DateTime? checkOut = null,
        Guid? guestProfileId = null, int? userId = 7) =>
        Booking.Place(userId.HasValue ? Guest(userId.Value, guestEmail) : BookingRequester.HotelStaff(1, HotelId),
            SampleRoom(roomId),
            new DateRange(checkIn ?? Today.AddDays(1), checkOut ?? Today.AddDays(3)),
            new GuestContact(guestName, guestEmail, null), userId, guestProfileId, Today, Now, PaymentHold);

    private class FakeBookingRepository : IBookingRepository
    {
        public Booking? SavedBooking { get; private set; }
        public Booking? BookingToReturn { get; set; }
        public bool UpdateCalled { get; private set; }

        public Task AddAsync(Booking entity)
        {
            SavedBooking = entity;
            return Task.CompletedTask;
        }

        public Task<Booking?> FindByIdAsync(int id) => Task.FromResult(BookingToReturn);

        public void Update(Booking entity)
        {
            UpdateCalled = true;
            SavedBooking = entity;
        }

        public void Remove(Booking entity) { }

        public Task<IEnumerable<Booking>> ListAsync() => Task.FromResult<IEnumerable<Booking>>(new List<Booking>());

        public Task<IEnumerable<Booking>> FindByOwnerAsync(int userId, Guid? guestProfileId) => Task.FromResult<IEnumerable<Booking>>(new List<Booking>());
        public Task<IEnumerable<Booking>> ListNewestFirstAsync(int? hotelId) => Task.FromResult<IEnumerable<Booking>>(new List<Booking>());
        public Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId) => Task.FromResult<IEnumerable<Booking>>(new List<Booking>());
        public Task<bool> ExistsActiveBookingOverlappingAsync(int roomId, DateRange dates, int? excludingBookingId = null) => Task.FromResult(false);
        public Task<IReadOnlySet<int>> FindRoomIdsWithActiveBookingOverlappingAsync(IReadOnlyCollection<int> roomIds, DateRange dates) => Task.FromResult<IReadOnlySet<int>>(new HashSet<int>());
        public Task<IReadOnlyList<Booking>> ListActiveOverlappingAsync(int? hotelId, DateRange window) => Task.FromResult<IReadOnlyList<Booking>>(Array.Empty<Booking>());
        public Task<IReadOnlyDictionary<int, int>> CountActiveByRoomAsync(IReadOnlyCollection<int> roomIds) => Task.FromResult<IReadOnlyDictionary<int, int>>(new Dictionary<int, int>());
        public Task<IReadOnlyList<Booking>> ListPendingPaymentDueAsync(DateTimeOffset now) => Task.FromResult<IReadOnlyList<Booking>>(Array.Empty<Booking>());
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public bool CompleteCalled { get; private set; }
        public Task CompleteAsync()
        {
            CompleteCalled = true;
            return Task.CompletedTask;
        }
        public Task ExecuteInTransactionAsync(Func<Task> work) => work();
    }

    private class FakeGuestProfilesContextFacade : IGuestProfilesContextFacade
    {
        public Func<int, Task<Guid?>>? FetchByUserIdHandler { get; set; }
        public Func<string, Task<Guid?>>? FetchByEmailHandler { get; set; }

        public int? LastCheckedUserId { get; private set; }
        public string? LastCheckedEmail { get; private set; }
        public bool FetchByUserIdCalled { get; private set; }
        public bool FetchByEmailCalled { get; private set; }

        public Task<Guid?> FetchGuestProfileIdByUserIdAsync(int userId)
        {
            FetchByUserIdCalled = true;
            LastCheckedUserId = userId;
            return FetchByUserIdHandler != null ? FetchByUserIdHandler(userId) : Task.FromResult<Guid?>(null);
        }

        public Task<Guid?> FetchGuestProfileIdByEmailAsync(string email)
        {
            FetchByEmailCalled = true;
            LastCheckedEmail = email;
            return FetchByEmailHandler != null ? FetchByEmailHandler(email) : Task.FromResult<Guid?>(null);
        }

        public Task<Guid?> CreateGuestProfileAsync(string firstName, string lastName, string phone, string? email = null, int? userId = null)
            => Task.FromResult<Guid?>(null);

        public Task<bool> LinkGuestProfileToUserAsync(Guid guestProfileId, int userId, string email)
            => Task.FromResult(true);
    }

    private class FakeIamContextFacade : IIamContextFacade
    {
        public Func<int, Task<UserContact?>>? FetchUserContactHandler { get; set; }
        public Task<UserContact?> FetchUserContactAsync(int userId)
        {
            if (FetchUserContactHandler != null) return FetchUserContactHandler(userId);
            var email = userId == 42 ? "alice@example.com" : userId == 999 ? "anonymous@guest.com" : "contact@example.com";
            return Task.FromResult<UserContact?>(new UserContact(userId, email, "User", "Guest"));
        }
        public Task<IReadOnlyList<UserContact>> ListHotelStaffAsync(int hotelId, IReadOnlyCollection<string> roles) => Task.FromResult<IReadOnlyList<UserContact>>(Array.Empty<UserContact>());
        public Task<int> FetchUserIdByEmail(string email) => Task.FromResult(1);
        public Task<string> FetchEmailByUserId(int userId) => Task.FromResult("guest@example.com");
        public Task<ReissuedSession?> AssignHotelToAdministratorAsync(int userId, int hotelId, SessionContext currentSession) => Task.FromResult<ReissuedSession?>(null);
    }

    private class FakeAccommodationsContextFacade : IAccommodationsContextFacade
    {
        public Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds) => Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string>());
        public Task<RoomOffer?> FetchRoomAsync(int roomId) => Task.FromResult<RoomOffer?>(SampleRoom(roomId));
        public Task<HotelSummary?> FetchHotelAsync(int hotelId) => Task.FromResult<HotelSummary?>(null);
        public Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int hotelId) => Task.FromResult<HotelPaymentInstructions?>(null);
        public Task<bool> HotelExistsAsync(int hotelId) => Task.FromResult(true);
        public Task<bool> RoomExistsAsync(int roomId) => Task.FromResult(true);
        public Task<decimal?> FetchRoomPricePerNightAsync(int roomId) => Task.FromResult<decimal?>(150m);
        public Task<int?> FetchHotelIdOfRoomAsync(int roomId) => Task.FromResult<int?>(1);
        public Task<RoomOffer?> LockRoomForBookingAsync(int roomId) => Task.FromResult<RoomOffer?>(SampleRoom(roomId));
        public Task OccupyRoomForCheckInAsync(int roomId, int? guestUserId, string? guestEmail) => Task.CompletedTask;
        public Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? hotelId) => Task.FromResult<IReadOnlyList<RoomOffer>>(Array.Empty<RoomOffer>());
    }

    private static BookingCommandService CreateService(
        IBookingRepository repo,
        IUnitOfWork uow,
        IGuestProfilesContextFacade guestProfiles)
    {
        var options = Options.Create(new BookingPolicySettings());
        var calendar = new HotelCalendar(options, TimeProvider.System);
        var availability = new RoomAvailabilityService(repo);
        return new BookingCommandService(
            repo,
            uow,
            guestProfiles,
            new FakeAccommodationsContextFacade(),
            new FakeIamContextFacade(),
            availability,
            calendar,
            options,
            NullLogger<BookingCommandService>.Instance);
    }

    [Test]
    public async Task CreateBooking_WithUserId_ShouldResolveGuestProfileIdViaUserId()
    {
        // Arrange
        var expectedProfileId = Guid.NewGuid();
        const int userId = 42;
        var repo = new FakeBookingRepository();
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade
        {
            FetchByUserIdHandler = id => Task.FromResult<Guid?>(id == userId ? expectedProfileId : null)
        };

        var service = CreateService(repo, uow, facade);
        var command = new CreateBookingCommand(
            Requester: Guest(userId, "alice@example.com"),
            RoomId: 101,
            GuestName: "Alice Wonderland",
            GuestEmail: "alice@example.com",
            CheckInDate: Today.AddDays(1),
            CheckOutDate: Today.AddDays(3),
            UserId: userId);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.GuestProfileId.Should().Be(expectedProfileId);
        result.GuestName.Should().Be("Alice Wonderland");
        result.GuestEmail.Should().Be("alice@example.com");
        facade.FetchByUserIdCalled.Should().BeTrue();
        facade.LastCheckedUserId.Should().Be(userId);
        uow.CompleteCalled.Should().BeTrue();
    }

    [Test]
    public async Task CreateBooking_WithoutUserId_WithEmail_ShouldResolveGuestProfileIdViaEmail()
    {
        // Arrange
        var expectedProfileId = Guid.NewGuid();
        const string email = "bob@example.com";
        var repo = new FakeBookingRepository();
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade
        {
            FetchByEmailHandler = e => Task.FromResult<Guid?>(e == email ? expectedProfileId : null)
        };

        var service = CreateService(repo, uow, facade);
        var staffRequester = BookingRequester.HotelStaff(1, HotelId);
        var command = new CreateBookingCommand(
            Requester: staffRequester,
            RoomId: 202,
            GuestName: "Bob Builder",
            GuestEmail: email,
            CheckInDate: Today.AddDays(2),
            CheckOutDate: Today.AddDays(5),
            UserId: null);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.GuestProfileId.Should().Be(expectedProfileId);
        result.GuestName.Should().Be("Bob Builder");
        result.GuestEmail.Should().Be(email);
        facade.FetchByEmailCalled.Should().BeTrue();
        facade.LastCheckedEmail.Should().Be(email);
        uow.CompleteCalled.Should().BeTrue();
    }

    [Test]
    public async Task CreateBooking_WhenGuestNotFoundInProfiles_ShouldCreateBookingWithNullGuestProfileId()
    {
        // Arrange
        var repo = new FakeBookingRepository();
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade
        {
            FetchByUserIdHandler = _ => Task.FromResult<Guid?>(null),
            FetchByEmailHandler = _ => Task.FromResult<Guid?>(null)
        };

        var service = CreateService(repo, uow, facade);
        var command = new CreateBookingCommand(
            Requester: Guest(999, "anonymous@guest.com"),
            RoomId: 101,
            GuestName: "Anonymous Guest",
            GuestEmail: "anonymous@guest.com",
            CheckInDate: Today.AddDays(1),
            CheckOutDate: Today.AddDays(2),
            UserId: 999);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.GuestProfileId.Should().BeNull();
        result.GuestName.Should().Be("Anonymous Guest");
        result.GuestEmail.Should().Be("anonymous@guest.com");
        facade.FetchByUserIdCalled.Should().BeTrue();
        uow.CompleteCalled.Should().BeTrue();
    }

    [Test]
    public async Task CreateBooking_WithExplicitGuestProfileId_ShouldNotQueryFacade()
    {
        // Arrange
        var explicitProfileId = Guid.NewGuid();
        var repo = new FakeBookingRepository();
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade();

        var service = CreateService(repo, uow, facade);
        var staffRequester = BookingRequester.HotelStaff(1, HotelId);
        var command = new CreateBookingCommand(
            Requester: staffRequester,
            RoomId: 101,
            GuestName: "Direct Profile Guest",
            GuestEmail: "direct@example.com",
            CheckInDate: Today.AddDays(1),
            CheckOutDate: Today.AddDays(2),
            UserId: 50,
            GuestProfileId: explicitProfileId);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.GuestProfileId.Should().Be(explicitProfileId);
        facade.FetchByUserIdCalled.Should().BeFalse();
        facade.FetchByEmailCalled.Should().BeFalse();
        uow.CompleteCalled.Should().BeTrue();
    }

    [Test]
    public async Task ConfirmBooking_ShouldUpdateBookingStatusToConfirmed()
    {
        // Arrange
        var booking = PlacePendingBooking(101, "John Doe", "john@example.com", Today.AddDays(1), Today.AddDays(3));

        var repo = new FakeBookingRepository { BookingToReturn = booking };
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade();

        var service = CreateService(repo, uow, facade);
        var command = new ConfirmBookingCommand(booking.Id);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Confirmed);
        uow.CompleteCalled.Should().BeTrue();
    }

    [Test]
    public async Task CancelBooking_ShouldUpdateBookingStatusToCancelled()
    {
        // Arrange
        var booking = PlacePendingBooking(101, "John Doe", "john@example.com", Today.AddDays(1), Today.AddDays(3), userId: 7);

        var repo = new FakeBookingRepository { BookingToReturn = booking };
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade();

        var service = CreateService(repo, uow, facade);
        var command = new CancelBookingCommand(booking.Id, Guest(7, "john@example.com"));

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Cancelled);
        uow.CompleteCalled.Should().BeTrue();
    }
}
