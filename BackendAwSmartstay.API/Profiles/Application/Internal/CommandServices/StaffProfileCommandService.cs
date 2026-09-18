using BackendAwSmartstay.Domain.Profiles.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;

public class StaffProfileCommandService(
    IStaffProfileRepository staffProfileRepository,
    IEmployeeCodeGenerator employeeCodeGenerator,
    IUnitOfWork unitOfWork,
    IDomainEventPublisher domainEventPublisher,
    IAccommodationsContextFacade accommodationsContextFacade) : IStaffProfileCommandService
{
    public async Task<StaffProfile?> Handle(CreateStaffProfileCommand command)
    {
        var code = await employeeCodeGenerator.GenerateNextCodeAsync();

        var staff = new StaffProfile(
            StaffProfileId.New(),
            command.UserId,
            code,
            command.Name,
            command.Email,
            command.Position,
            command.Shift,
            command.Phone,
            command.Address,
            command.Document);

        await staffProfileRepository.AddAsync(staff);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(staff.DomainEvents);
        staff.ClearDomainEvents();

        return staff;
    }

    public async Task<StaffProfile?> Handle(AddStaffAssignmentCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        if (command.Scope == ScopeLevel.Hotel)
        {
            var hotelExists = await accommodationsContextFacade.HotelExistsAsync(command.TargetId.Value);
            if (!hotelExists)
            {
                throw new DomainValidationException(ProfileErrorCodes.HotelNotFound, $"Hotel with ID {command.TargetId.Value} does not exist in Accommodations.");
            }
        }

        staff.AddAssignment(command.Scope, command.TargetId, command.Role, command.Period, command.Today);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(staff.DomainEvents);
        staff.ClearDomainEvents();

        return staff;
    }

    public async Task<StaffProfile?> Handle(TerminateStaffAssignmentCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.TerminateAssignment(command.AssignmentId, command.TerminationDate);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(staff.DomainEvents);
        staff.ClearDomainEvents();

        return staff;
    }

    public async Task<StaffProfile?> Handle(SuspendStaffAssignmentCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.SuspendAssignment(command.AssignmentId);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(ReactivateStaffAssignmentCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.ReactivateAssignment(command.AssignmentId, command.Today);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(ChangeStaffLegalNameCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.ChangeLegalName(command.NewName);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(UpdateStaffPersonalContactCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.UpdatePersonalContact(command.Phone, command.Address);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(ChangeStaffJobPositionCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.ChangeJobPosition(command.NewPosition);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(ChangeStaffHabitualShiftCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.ChangeHabitualShift(command.NewShift);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(UpdateStaffIdentificationCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.UpdateIdentification(command.Document);

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(DeactivateStaffProfileCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.Deactivate();

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<StaffProfile?> Handle(ActivateStaffProfileCommand command)
    {
        var staff = await staffProfileRepository.FindByIdAsync(command.StaffProfileId);
        if (staff is null) return null;

        staff.Activate();

        staffProfileRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }
}
