using System;
using System.Linq;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Infrastructure.Persistence;

public class ProfilesPersistenceTests
{
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GuestProfile_ShouldPersist_AndMaterializeCorrectly_WithoutDispatchedCreationEvents()
    {
        var options = CreateNewContextOptions();
        var guestId = GuestProfileId.New();
        var userId = new UserId(1);
        var name = new PersonName("John", "Doe");
        var phone = new PhoneNumber("+1234567890");
        var email = new EmailAddress("john.doe@example.com");
        var document = new IdentificationDocument(DocumentType.Dni, "12345678");
        var address = new StreetAddress("Main St", "123", "Springfield", "12345", "USA");

        // 1. Create and Persist
        using (var context = new AppDbContext(options))
        {
            var repo = new GuestProfileRepository(context);
            var guest = new GuestProfile(guestId, name, phone, email, document, address, userId);
            
            // Verify creation event exists before persistence
            guest.DomainEvents.Should().HaveCount(1);

            await repo.AddAsync(guest);
            await context.SaveChangesAsync();
        }

        // 2. Query in a separate DbContext instance to test EF Core materialization
        using (var context = new AppDbContext(options))
        {
            var repo = new GuestProfileRepository(context);
            var loadedGuest = await repo.FindByIdAsync(guestId);

            loadedGuest.Should().NotBeNull();
            loadedGuest!.Id.Should().Be(guestId);
            loadedGuest.UserId.Should().Be(userId);
            loadedGuest.Name.FirstName.Should().Be("John");
            loadedGuest.Name.LastName.Should().Be("Doe");
            loadedGuest.Phone.Value.Should().Be("+1234567890");
            loadedGuest.Email!.Address.Should().Be("john.doe@example.com");
            loadedGuest.Document!.Type.Should().Be(DocumentType.Dni);
            loadedGuest.Document!.Number.Should().Be("12345678");
            loadedGuest.Address!.Street.Should().Be("Main St");
            loadedGuest.Status.Should().Be(ProfileStatus.Active);

            // Crucial architectural rule: Materializing an entity from persistence MUST NOT fire creation events
            loadedGuest.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GuestProfile_Queries_ByEmail_ByUserId_ByDocument_ShouldWorkCorrectly()
    {
        var options = CreateNewContextOptions();
        var guestId = GuestProfileId.New();
        var userId = new UserId(2);
        var email = new EmailAddress("lookup@smartstay.com");
        var document = new IdentificationDocument(DocumentType.Passport, "PASS998877");

        using (var context = new AppDbContext(options))
        {
            var repo = new GuestProfileRepository(context);
            var guest = new GuestProfile(
                guestId,
                new PersonName("Alice", "Wonderland"),
                new PhoneNumber("+51999888777"),
                email,
                document,
                userId: userId);

            await repo.AddAsync(guest);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options))
        {
            var repo = new GuestProfileRepository(context);
            
            var byEmail = await repo.FindByEmailAsync(email);
            byEmail.Should().NotBeNull();
            byEmail!.Id.Should().Be(guestId);

            var byUserId = await repo.FindByUserIdAsync(userId);
            byUserId.Should().NotBeNull();
            byUserId!.Id.Should().Be(guestId);

            var byDoc = await repo.FindByDocumentAsync(document);
            byDoc.Should().NotBeNull();
            byDoc!.Id.Should().Be(guestId);

            var existsEmail = await repo.ExistsByEmailAsync(email);
            existsEmail.Should().BeTrue();

            var existsUser = await repo.ExistsByUserIdAsync(userId);
            existsUser.Should().BeTrue();
        }
    }

    [Fact]
    public async Task StaffProfile_AndAssignments_ShouldPersist_AndMaterializeCorrectly()
    {
        var options = CreateNewContextOptions();
        var staffId = StaffProfileId.New();
        var userId = new UserId(10);
        var code = new EmployeeCode("EMP-00001");
        var name = new PersonName("Jane", "Smith");
        var email = new EmailAddress("jane.smith@smartstay.com");
        var position = new JobPosition("Manager");
        var shift = HabitualShift.Morning;
        var phone = new PhoneNumber("+1987654321");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetId = new TargetId(101);

        // 1. Create and Persist with Assignment
        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);
            var staff = new StaffProfile(staffId, userId, code, name, email, position, shift, phone);
            staff.AddAssignment(ScopeLevel.Hotel, targetId, StaffRole.Admin, new DateRange(today, today.AddMonths(6)), today);

            await repo.AddAsync(staff);
            await context.SaveChangesAsync();
        }

        // 2. Query in a separate DbContext instance
        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);
            var loadedStaff = await repo.FindByIdAsync(staffId);

            loadedStaff.Should().NotBeNull();
            loadedStaff!.Id.Should().Be(staffId);
            loadedStaff.UserId.Should().Be(userId);
            loadedStaff.Code.Value.Should().Be("EMP-00001");
            loadedStaff.Position.Value.Should().Be("Manager");
            loadedStaff.Shift.Should().Be(HabitualShift.Morning);
            loadedStaff.Assignments.Should().HaveCount(1);

            var assignment = loadedStaff.Assignments.First();
            assignment.Scope.Should().Be(ScopeLevel.Hotel);
            assignment.TargetId.Should().Be(targetId);
            assignment.Role.Should().Be(StaffRole.Admin);
            assignment.Status.Should().Be(AssignmentStatus.Active);

            // Crucial architectural check
            loadedStaff.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task StaffProfile_Queries_ByCode_ByUserId_ByTargetId_ShouldWorkCorrectly()
    {
        var options = CreateNewContextOptions();
        var staffId = StaffProfileId.New();
        var userId = new UserId(20);
        var code = new EmployeeCode("EMP-00042");
        var targetHotelId = new TargetId(201);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);
            var staff = new StaffProfile(
                staffId,
                userId,
                code,
                new PersonName("Bob", "Staff"),
                new EmailAddress("bob@smartstay.com"),
                new JobPosition("Supervisor"),
                HabitualShift.Rotating);

            staff.AddAssignment(ScopeLevel.Hotel, targetHotelId, StaffRole.Admin, new DateRange(today, today.AddMonths(12)), today);

            await repo.AddAsync(staff);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);

            var byCode = await repo.FindByEmployeeCodeAsync(code);
            byCode.Should().NotBeNull();
            byCode!.Id.Should().Be(staffId);

            var byUser = await repo.FindByUserIdAsync(userId);
            byUser.Should().NotBeNull();
            byUser!.Id.Should().Be(staffId);

            var byTarget = await repo.FindByTargetIdAsync(targetHotelId);
            byTarget.Should().ContainSingle(s => s.Id == staffId);

            var existsCode = await repo.ExistsByEmployeeCodeAsync(code);
            existsCode.Should().BeTrue();

            var existsUser = await repo.ExistsByUserIdAsync(userId);
            existsUser.Should().BeTrue();
        }
    }

    [Fact]
    public async Task EmployeeCodeGenerator_ShouldGenerateSequentially()
    {
        var options = CreateNewContextOptions();
        var userId1 = new UserId(30);

        using (var context = new AppDbContext(options))
        {
            var repo = new StaffProfileRepository(context);
            var generator = new EmployeeCodeGenerator(context);

            var code1 = await generator.GenerateNextCodeAsync();
            code1.Value.Should().Be("EMP-00001");

            var staff1 = new StaffProfile(
                StaffProfileId.New(),
                userId1,
                code1,
                new PersonName("First", "Staff"),
                new EmailAddress("staff1@smartstay.com"),
                new JobPosition("Receptionist"),
                HabitualShift.Morning);

            await repo.AddAsync(staff1);
            await context.SaveChangesAsync();

            var code2 = await generator.GenerateNextCodeAsync();
            code2.Value.Should().Be("EMP-00002");
        }
    }
}
