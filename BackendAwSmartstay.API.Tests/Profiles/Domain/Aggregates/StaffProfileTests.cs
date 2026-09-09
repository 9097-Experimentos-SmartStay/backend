using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Events;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Domain.Aggregates;

public class StaffProfileTests
{
    private readonly UserId _userId = new(1);
    private readonly EmployeeCode _code = new("EMP-00001");
    private readonly PersonName _name = new("Rosa", "Perez");
    private readonly EmailAddress _email = new("rosa@smartstay.com");
    private readonly JobPosition _position = new("Recepcionista");
    private readonly HabitualShift _shift = HabitualShift.Morning;
    private readonly DateOnly _today = new(2026, 9, 8);
    private readonly TargetId _hotelId = new(101);
    private readonly TargetId _chainId = new(999);

    private StaffProfile CreateValidStaff() =>
        new(StaffProfileId.New(), _userId, _code, _name, _email, _position, _shift);

    [Fact]
    public void Create_WithValidParameters_ShouldInstantiateAndPublishCreatedEvent()
    {
        // Act
        var staff = CreateValidStaff();

        // Assert
        staff.Status.Should().Be(ProfileStatus.Active);
        staff.Assignments.Should().BeEmpty();
        staff.DomainEvents.Should().ContainSingle(e => e is StaffProfileCreatedEvent);
    }

    [Fact]
    public void AddAssignment_HotelScopeWithValidRole_ShouldAddAndPublishCreatedEvent()
    {
        // Arrange
        var staff = CreateValidStaff();
        var period = new DateRange(_today, _today.AddMonths(6));

        // Act
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);

        // Assert
        staff.Assignments.Should().HaveCount(1);
        staff.Assignments.First().Role.Should().Be(StaffRole.Reception);
        staff.Assignments.First().Status.Should().Be(AssignmentStatus.Active);
        staff.DomainEvents.Should().Contain(e => e is StaffAssignmentCreatedEvent);
    }

    [Fact]
    public void AddAssignment_ChainScopeWithChainAdmin_ShouldBeAllowed()
    {
        // Arrange
        var staff = CreateValidStaff();
        var period = new DateRange(_today, _today.AddMonths(12));

        // Act
        staff.AddAssignment(ScopeLevel.Chain, _chainId, StaffRole.ChainAdmin, period, _today);

        // Assert
        staff.Assignments.Should().HaveCount(1);
        staff.Assignments.First().Role.Should().Be(StaffRole.ChainAdmin);
    }

    [Fact]
    public void AddAssignment_SecondChainAdmin_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var staff = CreateValidStaff();
        var period1 = new DateRange(_today, _today.AddMonths(12));
        staff.AddAssignment(ScopeLevel.Chain, _chainId, StaffRole.ChainAdmin, period1, _today);

        var secondChainId = new TargetId(888);
        var period2 = new DateRange(_today.AddMonths(1), _today.AddMonths(12));

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Chain, secondChainId, StaffRole.ChainAdmin, period2, _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ChainAdmin*");
    }

    [Fact]
    public void AddAssignment_ChainScopeWithNonChainAdminRole_ShouldThrowArgumentException()
    {
        // Arrange
        var staff = CreateValidStaff();
        var period = new DateRange(_today, _today.AddMonths(6));

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Chain, _chainId, StaffRole.Reception, period, _today);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Only ChainAdmin*");
    }

    [Fact]
    public void AddAssignment_HotelScopeWithChainAdminRole_ShouldThrowArgumentException()
    {
        // Arrange
        var staff = CreateValidStaff();
        var period = new DateRange(_today, _today.AddMonths(6));

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.ChainAdmin, period, _today);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ChainAdmin role is not permitted at Hotel scope*");
    }

    [Fact]
    public void AddAssignment_WhenStaffHasActiveChainAdmin_CannotAddHotelAssignment()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Chain, _chainId, StaffRole.ChainAdmin, new DateRange(_today), _today);

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Admin, new DateRange(_today), _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot take Hotel assignments*");
    }

    [Fact]
    public void AddAssignment_WhenStaffHasHotelAssignment_CannotAddChainAdmin()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Chain, _chainId, StaffRole.ChainAdmin, new DateRange(_today), _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*existing Hotel assignments*");
    }

    [Fact]
    public void AddAssignment_AdminAndReceptionInSameHotel_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Admin, new DateRange(_today), _today);

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already holds Admin duties*");
    }

    [Fact]
    public void AddAssignment_CompatibleOperationalRolesInSameHotel_ShouldBeAllowed()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today), _today);

        // Act
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Maintenance, new DateRange(_today), _today);

        // Assert
        staff.Assignments.Should().HaveCount(2);
    }

    [Fact]
    public void AddAssignment_DuplicateCurrentAssignment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today), _today);

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today), _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*identical scope, target, and role already exists*");
    }

    [Fact]
    public void AddAssignment_WhenPreviousAssignmentIsSuspended_ShouldStillBlockEquivalentDuplicate()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today), _today);
        var assignmentId = staff.Assignments.First().Id;
        staff.SuspendAssignment(assignmentId);

        // Act - SUSPENDED no se comporta como TERMINATED, sigue bloqueando duplicados
        var act = () => staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today), _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*identical scope, target, and role already exists*");
    }

    [Fact]
    public void AddAssignment_WhenPreviousAssignmentIsTerminated_AllowsNewEquivalentAssignment()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today), _today);
        var assignmentId = staff.Assignments.First().Id;
        staff.TerminateAssignment(assignmentId, _today);

        // Act - TERMINATED es histórico, no bloquea recontratación en nuevo assignment
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, new DateRange(_today.AddDays(1)), _today);

        // Assert
        staff.Assignments.Should().HaveCount(2);
        staff.Assignments.Count(a => a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Scheduled).Should().Be(1);
    }

    [Fact]
    public void AddAssignment_FutureStartDate_ShouldInstantiateAsScheduled()
    {
        // Arrange
        var staff = CreateValidStaff();
        var futurePeriod = new DateRange(_today.AddDays(10), _today.AddMonths(3));

        // Act
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Housekeeping, futurePeriod, _today);

        // Assert
        staff.Assignments.First().Status.Should().Be(AssignmentStatus.Scheduled);
    }

    [Fact]
    public void AddAssignment_WhenStaffIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.Deactivate();

        // Act
        var act = () => staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Staff, new DateRange(_today), _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void Deactivate_ShouldSetProfileInactiveAndSuspendActiveAndScheduledAssignments()
    {
        // Arrange
        var staff = CreateValidStaff();
        var hotel2Id = new TargetId(102);
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);
        staff.AddAssignment(ScopeLevel.Hotel, hotel2Id, StaffRole.Housekeeping, new DateRange(_today.AddDays(5)), _today);

        // Act
        staff.Deactivate();

        // Assert
        staff.Status.Should().Be(ProfileStatus.Inactive);
        staff.Assignments.Should().OnlyContain(a => a.Status == AssignmentStatus.Suspended);
    }

    [Fact]
    public void Activate_ShouldNotAutomaticallyReactivateAssignments()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);
        staff.Deactivate();

        // Act
        staff.Activate();

        // Assert
        staff.Status.Should().Be(ProfileStatus.Active);
        staff.Assignments.First().Status.Should().Be(AssignmentStatus.Suspended);
    }

    [Fact]
    public void TerminateAssignment_WithValidId_ShouldTerminateAndEmitTerminatedEvent()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);
        var assignmentId = staff.Assignments.First().Id;

        // Act
        staff.TerminateAssignment(assignmentId, _today);

        // Assert
        staff.Assignments.First().Status.Should().Be(AssignmentStatus.Terminated);
        staff.DomainEvents.Should().Contain(e => e is StaffAssignmentTerminatedEvent);
    }

    [Fact]
    public void PersonalAndLaborUpdates_WhenActive_ShouldMutateCorrectly()
    {
        // Arrange
        var staff = CreateValidStaff();
        var newPosition = new JobPosition("Jefe de Recepcion");
        var newShift = HabitualShift.Night;
        var newName = new PersonName("Rosa Maria", "Perez Gomez");
        var newPhone = new PhoneNumber("+51988888888");
        var newDoc = new IdentificationDocument(DocumentType.Dni, "77778888");

        // Act
        staff.ChangeJobPosition(newPosition);
        staff.ChangeHabitualShift(newShift);
        staff.ChangeLegalName(newName);
        staff.UpdatePersonalContact(newPhone, null);
        staff.UpdateIdentification(newDoc);

        // Assert
        staff.Position.Should().Be(newPosition);
        staff.Shift.Should().Be(newShift);
        staff.Name.Should().Be(newName);
        staff.Phone.Should().Be(newPhone);
        staff.Document.Should().Be(newDoc);
    }

    [Fact]
    public void AdministrativeOperations_WhenStaffInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.AddAssignment(ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);
        var assignmentId = staff.Assignments.First().Id;
        staff.Deactivate();

        // Act & Assert
        var act1 = () => staff.ChangeJobPosition(new JobPosition("Supervisor"));
        var act2 = () => staff.ChangeLegalName(new PersonName("Test", "User"));
        var act3 = () => staff.TerminateAssignment(assignmentId, _today);
        var act4 = () => staff.ReactivateAssignment(assignmentId, _today);

        act1.Should().Throw<InvalidOperationException>().WithMessage("*inactive*");
        act2.Should().Throw<InvalidOperationException>().WithMessage("*inactive*");
        act3.Should().Throw<InvalidOperationException>().WithMessage("*inactive*");
        act4.Should().Throw<InvalidOperationException>().WithMessage("*inactive*");
    }
}
