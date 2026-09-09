using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.Events;

public record GuestProfileCreatedEvent(GuestProfileId ProfileId, string FullName, string? Email, string Phone, DateTimeOffset OccurredOn = default) : IEvent
{
    public DateTimeOffset OccurredOn { get; init; } = OccurredOn == default ? DateTimeOffset.UtcNow : OccurredOn;
}

public record GuestLinkedToUserEvent(GuestProfileId ProfileId, UserId UserId, string Email, DateTimeOffset OccurredOn = default) : IEvent
{
    public DateTimeOffset OccurredOn { get; init; } = OccurredOn == default ? DateTimeOffset.UtcNow : OccurredOn;
}

public record GuestIdentificationCorrectedEvent(
    GuestProfileId ProfileId,
    IdentificationDocument OldDocument,
    IdentificationDocument NewDocument,
    string Reason,
    UserId CorrectedByStaffUserId,
    DateTimeOffset OccurredOn) : IEvent;

public record StaffProfileCreatedEvent(StaffProfileId ProfileId, UserId UserId, string EmployeeCode, string Email, DateTimeOffset OccurredOn = default) : IEvent
{
    public DateTimeOffset OccurredOn { get; init; } = OccurredOn == default ? DateTimeOffset.UtcNow : OccurredOn;
}

public record StaffAssignmentCreatedEvent(
    StaffProfileId ProfileId,
    UserId UserId,
    AssignmentId AssignmentId,
    ScopeLevel Scope,
    TargetId TargetId,
    StaffRole Role,
    AssignmentStatus Status,
    DateTimeOffset OccurredOn = default) : IEvent
{
    public DateTimeOffset OccurredOn { get; init; } = OccurredOn == default ? DateTimeOffset.UtcNow : OccurredOn;
}

public record StaffAssignmentTerminatedEvent(
    StaffProfileId ProfileId,
    UserId UserId,
    AssignmentId AssignmentId,
    TargetId TargetId,
    StaffRole Role,
    DateTimeOffset OccurredOn = default) : IEvent
{
    public DateTimeOffset OccurredOn { get; init; } = OccurredOn == default ? DateTimeOffset.UtcNow : OccurredOn;
}

