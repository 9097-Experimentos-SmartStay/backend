using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

public static class CreateRoomCommandFromResourceAssembler
{
    public static CreateRoomCommand ToCommandFromResource(CreateRoomResource resource)
    {
        return new CreateRoomCommand(
            resource.HotelId,
            resource.RoomTypeId,
            resource.Price,
            resource.Description,
            resource.Amenities
        );
    }
}