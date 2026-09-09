using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST.Transform;

public static class GuestResourceAssembler
{
    public static GuestProfileResource ToResourceFromEntity(GuestProfile entity)
    {
        return new GuestProfileResource(
            entity.Id.Value,
            entity.UserId?.Value,
            entity.Name.FirstName,
            entity.Name.LastName,
            entity.Name.FullName,
            entity.Email?.Address,
            entity.Phone.Value,
            entity.Document?.Type,
            entity.Document?.Number,
            entity.Address?.Street,
            entity.Address?.Number,
            entity.Address?.City,
            entity.Address?.PostalCode,
            entity.Address?.Country,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static CreateGuestProfileCommand ToCommandFromResource(CreateGuestProfileResource resource)
    {
        return new CreateGuestProfileCommand(
            resource.FirstName,
            resource.LastName,
            resource.Phone,
            resource.Email,
            resource.DocumentType,
            resource.DocumentNumber,
            resource.Street,
            resource.Number,
            resource.City,
            resource.PostalCode,
            resource.Country,
            resource.UserId.HasValue ? new UserId(resource.UserId.Value) : null);
    }
}
