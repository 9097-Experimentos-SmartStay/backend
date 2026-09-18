using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

public static class UpdateRoomCommandFromResourceAssembler
{
    public static UpdateRoomCommand ToCommandFromResource(int roomId, UpdateRoomResource resource) =>
        new(roomId, resource.RoomTypeId, resource.Price, resource.Description!.Trim(), resource.Amenities ?? [],
            resource.Number);
}
