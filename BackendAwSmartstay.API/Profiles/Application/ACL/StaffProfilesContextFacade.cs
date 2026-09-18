using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;

namespace BackendAwSmartstay.API.Profiles.Application.ACL;

public class StaffProfilesContextFacade(
    IStaffProfileCommandService staffProfileCommandService,
    IStaffProfileQueryService staffProfileQueryService) : IStaffProfilesContextFacade
{
    public async Task<Guid?> CreateStaffProfileAsync(
        int userId,
        string firstName,
        string lastName,
        string email,
        string position,
        string shift)
    {
        if (!Enum.TryParse<HabitualShift>(shift, true, out var shiftEnum))
            throw new DomainValidationException($"Invalid shift value: {shift}");

        var command = new CreateStaffProfileCommand(
            new UserId(userId),
            new PersonName(firstName, lastName),
            new EmailAddress(email),
            new JobPosition(position),
            shiftEnum);

        var staff = await staffProfileCommandService.Handle(command);
        return staff?.Id.Value;
    }

    public async Task<Guid?> FetchStaffProfileIdByUserIdAsync(int userId)
    {
        var query = new GetStaffProfileByUserIdQuery(new UserId(userId));
        var staff = await staffProfileQueryService.Handle(query);
        return staff?.Id.Value;
    }

    public async Task<bool> HasActiveRoleInHotelAsync(int userId, int hotelId, string requiredRole)
    {
        if (!Enum.TryParse<StaffRole>(requiredRole, true, out var roleEnum))
            return false;

        var query = new GetStaffProfileByUserIdQuery(new UserId(userId));
        var staff = await staffProfileQueryService.Handle(query);
        if (staff is null || staff.Status != ProfileStatus.Active) return false;

        var targetId = new TargetId(hotelId);
        return staff.Assignments.Any(a =>
            a.IsCurrentOrScheduled() &&
            a.TargetId == targetId &&
            a.Role == roleEnum);
    }

    public async Task<bool> IsChainAdminAsync(int userId)
    {
        var query = new GetStaffProfileByUserIdQuery(new UserId(userId));
        var staff = await staffProfileQueryService.Handle(query);
        if (staff is null || staff.Status != ProfileStatus.Active) return false;

        return staff.Assignments.Any(a =>
            a.IsCurrentOrScheduled() &&
            a.Scope == ScopeLevel.Chain &&
            a.Role == StaffRole.ChainAdmin);
    }
}
