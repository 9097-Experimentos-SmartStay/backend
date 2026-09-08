using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert UpdateRoomResource to UpdateRoomCommand.
/// </summary>
public static class UpdateRoomCommandFromResourceAssembler
{
    public static UpdateRoomCommand ToCommandFromResource(int roomId, UpdateRoomResource resource)
    {
        return new UpdateRoomCommand(
            roomId,
            resource.RoomTypeId,
            resource.Price,
            resource.Description,
            resource.Amenities
        );
    }
}