using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

public static class CreateRoomTypeCommandFromResourceAssembler
{
    public static CreateRoomTypeCommand ToCommandFromResource(CreateRoomTypeResource resource)
    {
        return new CreateRoomTypeCommand(resource.Name, resource.Description);
    }
}

