using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.Commands;

public record CreateStaffProfileCommand(
    UserId UserId,
    PersonName Name,
    EmailAddress Email,
    JobPosition Position,
    HabitualShift Shift,
    PhoneNumber? Phone = null,
    StreetAddress? Address = null,
    IdentificationDocument? Document = null);

public record AddStaffAssignmentCommand(
    StaffProfileId StaffProfileId,
    ScopeLevel Scope,
    TargetId TargetId,
    StaffRole Role,
    DateRange Period,
    DateOnly Today);

public record TerminateStaffAssignmentCommand(
    StaffProfileId StaffProfileId,
    AssignmentId AssignmentId,
    DateOnly TerminationDate);

public record SuspendStaffAssignmentCommand(
    StaffProfileId StaffProfileId,
    AssignmentId AssignmentId);

public record ReactivateStaffAssignmentCommand(
    StaffProfileId StaffProfileId,
    AssignmentId AssignmentId,
    DateOnly Today);

public record ChangeStaffLegalNameCommand(
    StaffProfileId StaffProfileId,
    PersonName NewName);

public record UpdateStaffPersonalContactCommand(
    StaffProfileId StaffProfileId,
    PhoneNumber? Phone,
    StreetAddress? Address);

public record ChangeStaffJobPositionCommand(
    StaffProfileId StaffProfileId,
    JobPosition NewPosition);

public record ChangeStaffHabitualShiftCommand(
    StaffProfileId StaffProfileId,
    HabitualShift NewShift);

public record UpdateStaffIdentificationCommand(
    StaffProfileId StaffProfileId,
    IdentificationDocument Document);

public record DeactivateStaffProfileCommand(
    StaffProfileId StaffProfileId);

public record ActivateStaffProfileCommand(
    StaffProfileId StaffProfileId);
