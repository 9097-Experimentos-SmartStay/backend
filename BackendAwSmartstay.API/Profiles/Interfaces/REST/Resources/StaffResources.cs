using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;

public record StaffAssignmentResource(
    Guid Id,
    string Scope,
    int TargetId,
    string Role,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status);

public record StaffProfileResource(
    Guid Id,
    int UserId,
    string Code,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string Position,
    string Shift,
    DocumentType? DocumentType,
    string? DocumentNumber,
    string? Street,
    string? Number,
    string? City,
    string? PostalCode,
    string? Country,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IEnumerable<StaffAssignmentResource> Assignments);

public record CreateStaffProfileResource(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string Position,
    HabitualShift Shift,
    string? Phone,
    DocumentType? DocumentType,
    string? DocumentNumber,
    string? Street,
    string? Number,
    string? City,
    string? PostalCode,
    string? Country);

public record AddStaffAssignmentResource(
    ScopeLevel Scope,
    int TargetId,
    StaffRole Role,
    DateOnly StartDate,
    DateOnly? EndDate);

public record TerminateStaffAssignmentResource(
    DateOnly TerminationDate);

public record ChangeStaffLegalNameResource(
    string FirstName,
    string LastName);

public record UpdateStaffPersonalContactResource(
    string? Phone,
    string? Street,
    string? Number,
    string? City,
    string? PostalCode,
    string? Country);

public record ChangeStaffJobPositionResource(
    string Position);

public record ChangeStaffHabitualShiftResource(
    HabitualShift Shift);

public record UpdateStaffIdentificationResource(
    DocumentType DocumentType,
    string DocumentNumber);
