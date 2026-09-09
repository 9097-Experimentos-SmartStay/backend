using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Profiles.Application.ACL;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Compatibility;

public class CrossContextIdentityCompatibilityTests
{
    private class FakeDomainEventPublisher : IDomainEventPublisher
    {
        public Task PublishAsync(IReadOnlyCollection<IEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void UserId_ShouldAcceptCanonicalIamUserId_AsInteger()
    {
        // Arrange: IAM canonical user ID is an int (e.g., auto-incremented primary key = 42)
        const int iamUserId = 42;

        // Act: Profiles value object wrapping canonical IAM user ID
        var profileUserId = new UserId(iamUserId);

        // Assert
        profileUserId.Value.Should().Be(iamUserId);
        profileUserId.Value.Should().BeOfType(typeof(int));
    }

    [Fact]
    public void TargetId_ShouldAcceptCanonicalAccommodationsHotelId_AsInteger()
    {
        // Arrange: Accommodations canonical hotel ID is an int (e.g., auto-incremented primary key = 101)
        const int hotelId = 101;

        // Act: Profiles TargetId wrapping canonical Hotel ID
        var targetId = new TargetId(hotelId);

        // Assert
        targetId.Value.Should().Be(hotelId);
        targetId.Value.Should().BeOfType(typeof(int));
    }

    [Fact]
    public async Task GuestProfile_LinkedToIamUser_ShouldPersistAndQueryByIntegerUserId()
    {
        var options = CreateNewContextOptions();
        const int iamUserId = 7;
        var guestId = GuestProfileId.New();

        using (var context = new AppDbContext(options))
        {
            var repo = new GuestProfileRepository(context);
            var guest = new GuestProfile(
                guestId,
                new PersonName("Carlos", "Gomez"),
                new PhoneNumber("+51987654321"),
                new EmailAddress("carlos.gomez@example.com"),
                new IdentificationDocument(DocumentType.Dni, "44556677"),
                new StreetAddress("Av. Javier Prado", "1234", "Lima", "15001", "Peru"),
                new UserId(iamUserId));

            await repo.AddAsync(guest);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options))
        {
            var repo = new GuestProfileRepository(context);
            var queryService = new GuestProfileQueryService(repo);
            var unitOfWork = new UnitOfWork(context);
            var eventPublisher = new FakeDomainEventPublisher();
            var commandService = new GuestProfileCommandService(repo, unitOfWork, eventPublisher);
            var facade = new GuestProfilesContextFacade(commandService, queryService);

            // Query by int userId using facade (ACL)
            var profileId = await facade.FetchGuestProfileIdByUserIdAsync(iamUserId);
            profileId.Should().NotBeNull();
            profileId.Should().Be(guestId.Value);

            // Query by int userId using query service
            var profile = await queryService.Handle(new API.Profiles.Application.Internal.Queries.GetGuestProfileByUserIdQuery(new UserId(iamUserId)));
            profile.Should().NotBeNull();
            profile!.UserId.Should().Be(new UserId(iamUserId));
            profile.Name.FullName.Should().Be("Carlos Gomez");
        }
    }

    [Fact]
    public async Task StaffProfile_AssignedToAccommodationsHotel_ShouldPersistAndQueryByIntegerHotelId()
    {
        var options = CreateNewContextOptions();
        const int iamUserId = 15;
        const int hotelId = 205;
        var staffId = StaffProfileId.New();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);
            var staff = new StaffProfile(
                staffId,
                new UserId(iamUserId),
                new EmployeeCode("EMP-00100"),
                new PersonName("Maria", "Rodriguez"),
                new EmailAddress("maria.rodriguez@smartstay.com"),
                new JobPosition("Receptionist"),
                HabitualShift.Morning,
                new PhoneNumber("+51911223344"));

            staff.AddAssignment(ScopeLevel.Hotel, new TargetId(hotelId), StaffRole.Reception, new DateRange(today, today.AddMonths(12)), today);

            await repo.AddAsync(staff);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);
            var queryService = new StaffProfileQueryService(repo);
            var facade = new StaffProfilesContextFacade(null!, queryService);

            // Query staff active role in int hotelId via facade
            var hasRole = await facade.HasActiveRoleInHotelAsync(iamUserId, hotelId, "Reception");
            hasRole.Should().BeTrue();

            // Query staff by int userId via facade
            var profileId = await facade.FetchStaffProfileIdByUserIdAsync(iamUserId);
            profileId.Should().NotBeNull();
            profileId.Should().Be(staffId.Value);

            // Query staff by int hotelId via query service
            var staffInHotel = (await queryService.Handle(new API.Profiles.Application.Internal.Queries.GetStaffProfilesByHotelIdQuery(new TargetId(hotelId)))).ToList();
            staffInHotel.Should().HaveCount(1);
            staffInHotel[0].Id.Should().Be(staffId);
            staffInHotel[0].UserId.Should().Be(new UserId(iamUserId));
            staffInHotel[0].Assignments.Should().ContainSingle(a => a.TargetId == new TargetId(hotelId));
        }
    }
}
