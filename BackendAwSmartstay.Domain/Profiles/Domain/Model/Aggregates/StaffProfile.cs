using BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Entities;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Events;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;

public class StaffProfile
{
    private readonly List<StaffAssignment> _assignments = [];
    private readonly List<IEvent> _domainEvents = [];

    public StaffProfileId Id { get; }
    public UserId UserId { get; }
    public EmployeeCode Code { get; }
    public PersonName Name { get; private set; }
    public EmailAddress Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public StreetAddress? Address { get; private set; }
    public IdentificationDocument? Document { get; private set; }
    public JobPosition Position { get; private set; }
    public HabitualShift Shift { get; private set; }
    public ProfileStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<StaffAssignment> Assignments => _assignments.AsReadOnly();
    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Required for EF Core materialization without triggering creation domain events
    internal StaffProfile()
    {
        Code = null!;
        Name = null!;
        Email = null!;
        Position = null!;
    }

    public StaffProfile(
        StaffProfileId id,
        UserId userId,
        EmployeeCode code,
        PersonName name,
        EmailAddress email,
        JobPosition position,
        HabitualShift shift,
        PhoneNumber? phone = null,
        StreetAddress? address = null,
        IdentificationDocument? document = null)
    {
        Id = id;
        UserId = userId;
        Code = code;
        Name = name;
        Email = email;
        Position = position;
        Shift = shift;
        Phone = phone;
        Address = address;
        Document = document;
        Status = ProfileStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new StaffProfileCreatedEvent(Id, UserId, Code.Value, Email.Address));
    }

    public void AddAssignment(ScopeLevel scope, TargetId targetId, StaffRole role, DateRange period, DateOnly today)
    {
        EnsureActive();

        if (scope == ScopeLevel.Chain && role != StaffRole.ChainAdmin)
            throw new DomainValidationException(ProfileErrorCodes.ChainScopeRequiresChainAdmin, "Only ChainAdmin role is allowed at Chain scope.");

        if (scope == ScopeLevel.Hotel && role == StaffRole.ChainAdmin)
            throw new DomainValidationException(ProfileErrorCodes.ChainAdminNotAllowedAtHotel, "ChainAdmin role is not permitted at Hotel scope.");

        if (role == StaffRole.ChainAdmin && _assignments.Any(a => a.IsCurrentOrScheduled() && a.Role == StaffRole.ChainAdmin))
            throw new BusinessRuleViolationException(ProfileErrorCodes.ChainAdminAlreadyAssigned, "A staff profile cannot have more than one active, scheduled, or suspended ChainAdmin assignment.");

        if (scope == ScopeLevel.Hotel && _assignments.Any(a => a.IsCurrentOrScheduled() && a.Role == StaffRole.ChainAdmin))
            throw new BusinessRuleViolationException(ProfileErrorCodes.ChainAdminCannotTakeHotelAssignment, "Staff with active/scheduled ChainAdmin role cannot take Hotel assignments.");

        if (scope == ScopeLevel.Chain && _assignments.Any(a => a.IsCurrentOrScheduled() && a.Scope == ScopeLevel.Hotel))
            throw new BusinessRuleViolationException(ProfileErrorCodes.HotelAssignmentsBlockChainAdmin, "Cannot assign ChainAdmin to a staff profile with existing Hotel assignments.");

        if (scope == ScopeLevel.Hotel)
        {
            if (role == StaffRole.Admin && _assignments.Any(a => a.IsCurrentOrScheduled() && a.TargetId == targetId && a.Role == StaffRole.Reception))
                throw new BusinessRuleViolationException(ProfileErrorCodes.AdminConflictsWithReception, "Cannot assign Admin role: Staff already has Reception duties in this hotel.");

            if (role == StaffRole.Reception && _assignments.Any(a => a.IsCurrentOrScheduled() && a.TargetId == targetId && a.Role == StaffRole.Admin))
                throw new BusinessRuleViolationException(ProfileErrorCodes.ReceptionConflictsWithAdmin, "Cannot assign Reception role: Staff already holds Admin duties in this hotel.");
        }

        if (_assignments.Any(a => a.IsCurrentOrScheduled() && a.Scope == scope && a.TargetId == targetId && a.Role == role))
            throw new BusinessRuleViolationException(ProfileErrorCodes.AssignmentDuplicated, "A current or scheduled assignment with the identical scope, target, and role already exists.");

        var assignment = new StaffAssignment(AssignmentId.New(), scope, targetId, role, period, today);
        _assignments.Add(assignment);
        UpdatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new StaffAssignmentCreatedEvent(Id, UserId, assignment.Id, scope, targetId, role, assignment.Status));
    }

    public void TerminateAssignment(AssignmentId assignmentId, DateOnly terminationDate)
    {
        EnsureActive();

        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId)
            ?? throw new EntityNotFoundException("Assignment", assignmentId.Value);

        assignment.Terminate(terminationDate);
        UpdatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new StaffAssignmentTerminatedEvent(Id, UserId, assignment.Id, assignment.TargetId, assignment.Role));
    }

    public void SuspendAssignment(AssignmentId assignmentId)
    {
        EnsureActive();

        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId)
            ?? throw new EntityNotFoundException("Assignment", assignmentId.Value);

        assignment.Suspend();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReactivateAssignment(AssignmentId assignmentId, DateOnly today)
    {
        EnsureActive();

        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId)
            ?? throw new EntityNotFoundException("Assignment", assignmentId.Value);

        assignment.Reactivate(today);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeLegalName(PersonName newName)
    {
        EnsureActive();
        Name = newName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePersonalContact(PhoneNumber? phone, StreetAddress? address)
    {
        EnsureActive();
        Phone = phone;
        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeJobPosition(JobPosition newPosition)
    {
        EnsureActive();
        Position = newPosition;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeHabitualShift(HabitualShift newShift)
    {
        EnsureActive();
        Shift = newShift;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateIdentification(IdentificationDocument document)
    {
        EnsureActive();
        Document = document;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = ProfileStatus.Inactive;
        foreach (var assignment in _assignments.Where(a => a.Status is AssignmentStatus.Active or AssignmentStatus.Scheduled))
        {
            assignment.Suspend();
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = ProfileStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureActive()
    {
        if (Status == ProfileStatus.Inactive)
            throw new BusinessRuleViolationException(ProfileErrorCodes.StaffProfileInactive, "Operation not permitted on an inactive staff profile.");
    }
}
