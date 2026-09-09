using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Bookings.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using FluentAssertions;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Bookings.Application;

public class BookingCommandServiceAclIntegrationTests
{
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
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public bool CompleteCalled { get; private set; }
        public Task CompleteAsync()
        {
            CompleteCalled = true;
            return Task.CompletedTask;
        }
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

    [Fact]
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

        var service = new BookingCommandService(repo, uow, facade);
        var command = new CreateBookingCommand(
            RoomId: 101,
            GuestName: "Alice Wonderland",
            GuestEmail: "alice@example.com",
            CheckInDate: DateTime.UtcNow.AddDays(1),
            CheckOutDate: DateTime.UtcNow.AddDays(3),
            UserId: userId);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.GuestProfileId.Should().Be(expectedProfileId);
        result.GuestName.Should().Be("Alice Wonderland");
        result.GuestEmail.Should().Be("alice@example.com");
        facade.FetchByUserIdCalled.Should().BeTrue();
        facade.LastCheckedUserId.Should().Be(userId);
        facade.FetchByEmailCalled.Should().BeFalse();
        uow.CompleteCalled.Should().BeTrue();
    }

    [Fact]
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

        var service = new BookingCommandService(repo, uow, facade);
        var command = new CreateBookingCommand(
            RoomId: 202,
            GuestName: "Bob Builder",
            GuestEmail: email,
            CheckInDate: DateTime.UtcNow.AddDays(2),
            CheckOutDate: DateTime.UtcNow.AddDays(5),
            UserId: null);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.GuestProfileId.Should().Be(expectedProfileId);
        result.GuestName.Should().Be("Bob Builder");
        result.GuestEmail.Should().Be(email);
        facade.FetchByUserIdCalled.Should().BeFalse();
        facade.FetchByEmailCalled.Should().BeTrue();
        facade.LastCheckedEmail.Should().Be(email);
        uow.CompleteCalled.Should().BeTrue();
    }

    [Fact]
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

        var service = new BookingCommandService(repo, uow, facade);
        var command = new CreateBookingCommand(
            RoomId: 101,
            GuestName: "Anonymous Guest",
            GuestEmail: "anonymous@guest.com",
            CheckInDate: DateTime.UtcNow.AddDays(1),
            CheckOutDate: DateTime.UtcNow.AddDays(2),
            UserId: 999);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.GuestProfileId.Should().BeNull();
        result.GuestName.Should().Be("Anonymous Guest");
        result.GuestEmail.Should().Be("anonymous@guest.com");
        facade.FetchByUserIdCalled.Should().BeTrue();
        uow.CompleteCalled.Should().BeTrue();
    }

    [Fact]
    public async Task CreateBooking_WithExplicitGuestProfileId_ShouldNotQueryFacade()
    {
        // Arrange
        var explicitProfileId = Guid.NewGuid();
        var repo = new FakeBookingRepository();
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade();

        var service = new BookingCommandService(repo, uow, facade);
        var command = new CreateBookingCommand(
            RoomId: 101,
            GuestName: "Direct Profile Guest",
            GuestEmail: "direct@example.com",
            CheckInDate: DateTime.UtcNow.AddDays(1),
            CheckOutDate: DateTime.UtcNow.AddDays(2),
            UserId: 50,
            GuestProfileId: explicitProfileId);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.GuestProfileId.Should().Be(explicitProfileId);
        facade.FetchByUserIdCalled.Should().BeFalse();
        facade.FetchByEmailCalled.Should().BeFalse();
        uow.CompleteCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmBooking_ShouldUpdateBookingStatusToConfirmed()
    {
        // Arrange
        var booking = new Booking(
            roomId: 101,
            guestName: "John Doe",
            guestEmail: "john@example.com",
            checkInDate: DateTime.UtcNow.AddDays(1),
            checkOutDate: DateTime.UtcNow.AddDays(3));

        var repo = new FakeBookingRepository { BookingToReturn = booking };
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade();

        var service = new BookingCommandService(repo, uow, facade);
        var command = new ConfirmBookingCommand(booking.Id);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(BookingStatus.Confirmed);
        repo.UpdateCalled.Should().BeTrue();
        uow.CompleteCalled.Should().BeTrue();
    }

    [Fact]
    public async Task CancelBooking_ShouldUpdateBookingStatusToCancelled()
    {
        // Arrange
        var booking = new Booking(
            roomId: 101,
            guestName: "John Doe",
            guestEmail: "john@example.com",
            checkInDate: DateTime.UtcNow.AddDays(1),
            checkOutDate: DateTime.UtcNow.AddDays(3));

        var repo = new FakeBookingRepository { BookingToReturn = booking };
        var uow = new FakeUnitOfWork();
        var facade = new FakeGuestProfilesContextFacade();

        var service = new BookingCommandService(repo, uow, facade);
        var command = new CancelBookingCommand(booking.Id);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(BookingStatus.Cancelled);
        repo.UpdateCalled.Should().BeTrue();
        uow.CompleteCalled.Should().BeTrue();
    }
}
