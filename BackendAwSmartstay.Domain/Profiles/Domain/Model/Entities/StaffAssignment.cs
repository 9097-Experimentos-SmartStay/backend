using BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.Entities;

public class StaffAssignment
{
    public AssignmentId Id { get; }
    public ScopeLevel Scope { get; }
    public TargetId TargetId { get; }
    public StaffRole Role { get; }
    public DateRange Period { get; private set; }
    public AssignmentStatus Status { get; private set; }

    // Required for EF Core materialization
    internal StaffAssignment() { Period = null!; }

    internal StaffAssignment(AssignmentId id, ScopeLevel scope, TargetId targetId, StaffRole role, DateRange period, DateOnly today)
    {
        if (period.EndDate.HasValue && period.EndDate.Value < today)
            throw new BusinessRuleViolationException(ProfileErrorCodes.AssignmentPeriodExpired, "Cannot create an assignment whose contractual period has already expired.");

        Id = id;
        Scope = scope;
        TargetId = targetId;
        Role = role;
        Period = period;

        Status = period.StartDate > today
            ? AssignmentStatus.Scheduled
            : AssignmentStatus.Active;
    }

    public bool IsCurrentOrScheduled() =>
        Status is AssignmentStatus.Active or AssignmentStatus.Scheduled or AssignmentStatus.Suspended;

    internal void Suspend()
    {
        if (Status == AssignmentStatus.Terminated)
            throw new BusinessRuleViolationException(ProfileErrorCodes.AssignmentTerminated, "Cannot suspend a terminated assignment.");

        Status = AssignmentStatus.Suspended;
    }

    internal void Reactivate(DateOnly today)
    {
        if (Status != AssignmentStatus.Suspended)
            throw new BusinessRuleViolationException(ProfileErrorCodes.AssignmentNotSuspended, "Only suspended assignments can be reactivated.");

        if (Period.EndDate.HasValue && today > Period.EndDate.Value)
            throw new BusinessRuleViolationException(ProfileErrorCodes.AssignmentPeriodExpired, "Cannot reactivate an assignment whose contractual period has already expired.");

        Status = Period.StartDate > today 
            ? AssignmentStatus.Scheduled 
            : AssignmentStatus.Active;
    }

    internal void Terminate(DateOnly terminationDate)
    {
        if (Status == AssignmentStatus.Terminated)
            throw new BusinessRuleViolationException(ProfileErrorCodes.AssignmentAlreadyTerminated, "Assignment is already terminated.");

        if (terminationDate < Period.StartDate)
            throw new DomainValidationException(ProfileErrorCodes.TerminationBeforeStart, "Termination date cannot be earlier than start date.");

        Period = new DateRange(Period.StartDate, terminationDate);
        Status = AssignmentStatus.Terminated;
    }
}
