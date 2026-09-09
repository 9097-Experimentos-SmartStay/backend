using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;

public interface IStaffProfileCommandService
{
    Task<StaffProfile?> Handle(CreateStaffProfileCommand command);
    Task<StaffProfile?> Handle(AddStaffAssignmentCommand command);
    Task<StaffProfile?> Handle(TerminateStaffAssignmentCommand command);
    Task<StaffProfile?> Handle(SuspendStaffAssignmentCommand command);
    Task<StaffProfile?> Handle(ReactivateStaffAssignmentCommand command);
    Task<StaffProfile?> Handle(ChangeStaffLegalNameCommand command);
    Task<StaffProfile?> Handle(UpdateStaffPersonalContactCommand command);
    Task<StaffProfile?> Handle(ChangeStaffJobPositionCommand command);
    Task<StaffProfile?> Handle(ChangeStaffHabitualShiftCommand command);
    Task<StaffProfile?> Handle(UpdateStaffIdentificationCommand command);
    Task<StaffProfile?> Handle(DeactivateStaffProfileCommand command);
    Task<StaffProfile?> Handle(ActivateStaffProfileCommand command);
}
