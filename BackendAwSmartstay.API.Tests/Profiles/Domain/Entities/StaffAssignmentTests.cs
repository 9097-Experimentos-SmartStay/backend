using BackendAwSmartstay.Domain.Profiles.Domain.Model.Entities;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Domain.Entities;

public class StaffAssignmentTests
{
    private readonly DateOnly _today = new(2026, 9, 8);
    private readonly TargetId _hotelId = new(101);

    [Fact]
    public void Create_WhenStartDateIsTodayOrPast_ShouldHaveActiveStatus()
    {
        // Arrange
        var period = new DateRange(_today, _today.AddMonths(6));

        // Act
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Active);
        assignment.IsCurrentOrScheduled().Should().BeTrue();
    }

    [Fact]
    public void Create_WhenStartDateIsFuture_ShouldHaveScheduledStatus()
    {
        // Arrange
        var period = new DateRange(_today.AddDays(7), _today.AddMonths(6));

        // Act
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Scheduled);
        assignment.IsCurrentOrScheduled().Should().BeTrue();
    }

    [Fact]
    public void Create_WhenEndDateAlreadyPassed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var pastStart = _today.AddDays(-30);
        var pastEnd = _today.AddDays(-5);
        var period = new DateRange(pastStart, pastEnd);

        // Act
        var act = () => new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public void Suspend_WhenActiveOrScheduled_ShouldTransitionToSuspended()
    {
        // Arrange
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);

        // Act
        assignment.Suspend();

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Suspended);
        assignment.IsCurrentOrScheduled().Should().BeTrue();
    }

    [Fact]
    public void Suspend_WhenTerminated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);
        assignment.Terminate(_today);

        // Act
        var act = () => assignment.Suspend();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*terminated*");
    }

    [Fact]
    public void Reactivate_WhenStartDateIsFuture_ShouldReactivateAsScheduled()
    {
        // Arrange
        var futureStart = _today.AddDays(10);
        var period = new DateRange(futureStart, futureStart.AddMonths(3));
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);
        assignment.Suspend();

        // Act
        assignment.Reactivate(_today);

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Scheduled);
    }

    [Fact]
    public void Reactivate_WhenStartDateReached_ShouldReactivateAsActive()
    {
        // Arrange
        var pastStart = _today.AddDays(-10);
        var period = new DateRange(pastStart, _today.AddMonths(3));
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);
        assignment.Suspend();

        // Act
        assignment.Reactivate(_today);

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Active);
    }

    [Fact]
    public void Reactivate_WhenNotSuspended_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);

        // Act
        var act = () => assignment.Reactivate(_today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only suspended*");
    }

    [Fact]
    public void Reactivate_WhenContractualEndDateAlreadyPassed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var pastStart = _today.AddDays(-30);
        var pastEnd = _today.AddDays(-5);
        var period = new DateRange(pastStart, pastEnd);
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, pastStart);
        assignment.Suspend();

        // Act
        var act = () => assignment.Reactivate(_today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public void Terminate_WhenActive_ShouldAdjustEndDateAndMarkTerminated()
    {
        // Arrange
        var period = new DateRange(_today.AddDays(-10), _today.AddMonths(6));
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, period, _today);

        // Act
        assignment.Terminate(_today);

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Terminated);
        assignment.Period.EndDate.Should().Be(_today);
        assignment.IsCurrentOrScheduled().Should().BeFalse();
    }

    [Fact]
    public void Terminate_WhenTerminationDatePrecedesStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = _today.AddDays(5);
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(startDate), _today);

        // Act
        var act = () => assignment.Terminate(startDate.AddDays(-1));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Terminate_WhenAlreadyTerminated_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var assignment = new StaffAssignment(AssignmentId.New(), ScopeLevel.Hotel, _hotelId, StaffRole.Reception, new DateRange(_today), _today);
        assignment.Terminate(_today);

        // Act
        var act = () => assignment.Terminate(_today);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already terminated*");
    }
}
