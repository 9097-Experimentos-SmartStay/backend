using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Services;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace BackendAwSmartstay.API.Tests.Profiles.Application;

[TestFixture]
public class StaffProfileCommandServiceAclIntegrationTests
{
    private class FakeStaffRepository : IStaffProfileRepository
    {
        public StaffProfile? StaffToReturn { get; set; }
        public bool UpdateCalled { get; private set; }

        public Task AddAsync(StaffProfile staffProfile) => Task.CompletedTask;
        public Task<StaffProfile?> FindByIdAsync(StaffProfileId profileId) => Task.FromResult(StaffToReturn);
        public Task<StaffProfile?> FindByUserIdAsync(UserId userId) => Task.FromResult(StaffToReturn);
        public Task<StaffProfile?> FindByEmployeeCodeAsync(EmployeeCode code) => Task.FromResult(StaffToReturn);
        public Task<IEnumerable<StaffProfile>> FindByTargetIdAsync(TargetId targetId) => Task.FromResult<IEnumerable<StaffProfile>>(new List<StaffProfile>());
        public Task<IEnumerable<StaffProfile>> ListAsync() => Task.FromResult<IEnumerable<StaffProfile>>(new List<StaffProfile>());
        public Task<bool> ExistsByEmployeeCodeAsync(EmployeeCode code) => Task.FromResult(false);
        public Task<bool> ExistsByUserIdAsync(UserId userId) => Task.FromResult(false);
        public void Update(StaffProfile staffProfile) { UpdateCalled = true; }
        public void Remove(StaffProfile staffProfile) { }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public bool CompleteCalled { get; private set; }
        public Task CompleteAsync() { CompleteCalled = true; return Task.CompletedTask; }
        public Task ExecuteInTransactionAsync(Func<Task> work) => work();
    }

    private class FakeEmployeeCodeGenerator : IEmployeeCodeGenerator
    {
        public Task<EmployeeCode> GenerateNextCodeAsync() => Task.FromResult(new EmployeeCode("EMP-00001"));
    }

    private class FakeDomainEventPublisher : IDomainEventPublisher
    {
        public Task PublishAsync(IReadOnlyCollection<IEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class FakeAccommodationsContextFacade : IAccommodationsContextFacade
    {
        public Func<int, Task<bool>>? HotelExistsHandler { get; set; }
        public int LastCheckedHotelId { get; private set; }
        public bool WasCalled { get; private set; }

        public Task<bool> HotelExistsAsync(int hotelId)
        {
            WasCalled = true;
            LastCheckedHotelId = hotelId;
            return HotelExistsHandler != null ? HotelExistsHandler(hotelId) : Task.FromResult(false);
        }

        public Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds) => Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string>());
        public Task<RoomOffer?> FetchRoomAsync(int roomId) => Task.FromResult<RoomOffer?>(null);
        public Task<HotelSummary?> FetchHotelAsync(int hotelId) => Task.FromResult<HotelSummary?>(null);
        public Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int hotelId) => Task.FromResult<HotelPaymentInstructions?>(null);
        public Task<bool> RoomExistsAsync(int roomId) => Task.FromResult(false);
        public Task<decimal?> FetchRoomPricePerNightAsync(int roomId) => Task.FromResult<decimal?>(null);
        public Task<int?> FetchHotelIdOfRoomAsync(int roomId) => Task.FromResult<int?>(null);
        public Task<RoomOffer?> LockRoomForBookingAsync(int roomId) => Task.FromResult<RoomOffer?>(null);
        public Task OccupyRoomForCheckInAsync(int roomId, int? guestUserId, string? guestEmail) => Task.CompletedTask;
        public Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? hotelId) => Task.FromResult<IReadOnlyList<RoomOffer>>(Array.Empty<RoomOffer>());
    }

    private static StaffProfile CreateSampleStaff()
    {
        return new StaffProfile(
            StaffProfileId.New(),
            new UserId(1),
            new EmployeeCode("EMP-00001"),
            new PersonName("Jane", "Doe"),
            new EmailAddress("jane.doe@smartstay.com"),
            new JobPosition("Manager"),
            HabitualShift.Morning);
    }

    [Test]
    public async Task AddAssignment_ScopeLevelHotel_ValidHotel_ShouldCreateAssignment()
    {
        // Arrange
        const int validHotelId = 101;
        var staff = CreateSampleStaff();
        var repo = new FakeStaffRepository { StaffToReturn = staff };
        var unitOfWork = new FakeUnitOfWork();
        var generator = new FakeEmployeeCodeGenerator();
        var publisher = new FakeDomainEventPublisher();
        var facade = new FakeAccommodationsContextFacade
        {
            HotelExistsHandler = id => Task.FromResult(id == validHotelId)
        };

        var service = new StaffProfileCommandService(repo, generator, unitOfWork, publisher, facade);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var command = new AddStaffAssignmentCommand(
            staff.Id,
            ScopeLevel.Hotel,
            new TargetId(validHotelId),
            StaffRole.Admin,
            new DateRange(today, today.AddMonths(3)),
            today);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        facade.WasCalled.Should().BeTrue();
        facade.LastCheckedHotelId.Should().Be(validHotelId);
        unitOfWork.CompleteCalled.Should().BeTrue();
        repo.UpdateCalled.Should().BeTrue();
        staff.Assignments.Should().ContainSingle(a => a.TargetId == new TargetId(validHotelId));
    }

    [Test]
    public async Task AddAssignment_ScopeLevelHotel_NonExistentHotel_ShouldThrowArgumentException_AndNotPersist()
    {
        // Arrange
        const int nonExistentHotelId = 999;
        var staff = CreateSampleStaff();
        var repo = new FakeStaffRepository { StaffToReturn = staff };
        var unitOfWork = new FakeUnitOfWork();
        var generator = new FakeEmployeeCodeGenerator();
        var publisher = new FakeDomainEventPublisher();
        var facade = new FakeAccommodationsContextFacade
        {
            HotelExistsHandler = _ => Task.FromResult(false)
        };

        var service = new StaffProfileCommandService(repo, generator, unitOfWork, publisher, facade);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var command = new AddStaffAssignmentCommand(
            staff.Id,
            ScopeLevel.Hotel,
            new TargetId(nonExistentHotelId),
            StaffRole.Admin,
            new DateRange(today, today.AddMonths(3)),
            today);

        // Act
        Func<Task> action = async () => await service.Handle(command);

        // Assert
        await action.Should().ThrowAsync<DomainValidationException>()
            .WithMessage($"*Hotel with ID {nonExistentHotelId} does not exist in Accommodations*");

        facade.WasCalled.Should().BeTrue();
        facade.LastCheckedHotelId.Should().Be(nonExistentHotelId);
        unitOfWork.CompleteCalled.Should().BeFalse();
        repo.UpdateCalled.Should().BeFalse();
        staff.Assignments.Should().BeEmpty();
    }

    [Test]
    public async Task AddAssignment_ScopeLevelChain_ShouldNotInvokeHotelExistsAsync()
    {
        // Arrange
        const int chainId = 10;
        var staff = CreateSampleStaff();
        var repo = new FakeStaffRepository { StaffToReturn = staff };
        var unitOfWork = new FakeUnitOfWork();
        var generator = new FakeEmployeeCodeGenerator();
        var publisher = new FakeDomainEventPublisher();
        var facade = new FakeAccommodationsContextFacade();

        var service = new StaffProfileCommandService(repo, generator, unitOfWork, publisher, facade);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var command = new AddStaffAssignmentCommand(
            staff.Id,
            ScopeLevel.Chain,
            new TargetId(chainId),
            StaffRole.ChainAdmin,
            new DateRange(today, today.AddMonths(12)),
            today);

        // Act
        var result = await service.Handle(command);

        // Assert
        result.Should().NotBeNull();
        facade.WasCalled.Should().BeFalse();
        unitOfWork.CompleteCalled.Should().BeTrue();
        staff.Assignments.Should().ContainSingle(a => a.TargetId == new TargetId(chainId));
    }
}
