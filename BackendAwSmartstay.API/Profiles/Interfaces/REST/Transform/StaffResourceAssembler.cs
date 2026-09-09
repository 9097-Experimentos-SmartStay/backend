using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Entities;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST.Transform;

public static class StaffResourceAssembler
{
    public static StaffProfileResource ToResourceFromEntity(StaffProfile entity)
    {
        return new StaffProfileResource(
            entity.Id.Value,
            entity.UserId.Value,
            entity.Code.Value,
            entity.Name.FirstName,
            entity.Name.LastName,
            entity.Name.FullName,
            entity.Email.Address,
            entity.Phone?.Value,
            entity.Position.Value,
            entity.Shift.ToString(),
            entity.Document?.Type,
            entity.Document?.Number,
            entity.Address?.Street,
            entity.Address?.Number,
            entity.Address?.City,
            entity.Address?.PostalCode,
            entity.Address?.Country,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Assignments.Select(ToAssignmentResource));
    }

    public static StaffAssignmentResource ToAssignmentResource(StaffAssignment assignment)
    {
        return new StaffAssignmentResource(
            assignment.Id.Value,
            assignment.Scope.ToString(),
            assignment.TargetId.Value,
            assignment.Role.ToString(),
            assignment.Period.StartDate,
            assignment.Period.EndDate,
            assignment.Status.ToString());
    }

    public static CreateStaffProfileCommand ToCommandFromResource(CreateStaffProfileResource resource)
    {
        var address = resource.Street != null && resource.Number != null && resource.City != null && resource.PostalCode != null && resource.Country != null
            ? new StreetAddress(resource.Street, resource.Number, resource.City, resource.PostalCode, resource.Country)
            : null;

        var document = resource.DocumentType.HasValue && resource.DocumentNumber != null
            ? new IdentificationDocument(resource.DocumentType.Value, resource.DocumentNumber)
            : null;

        return new CreateStaffProfileCommand(
            new UserId(resource.UserId),
            new PersonName(resource.FirstName, resource.LastName),
            new EmailAddress(resource.Email),
            new JobPosition(resource.Position),
            resource.Shift,
            resource.Phone != null ? new PhoneNumber(resource.Phone) : null,
            address,
            document);
    }
}
