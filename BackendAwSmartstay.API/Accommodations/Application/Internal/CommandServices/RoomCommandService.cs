using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Accommodations.Application.Internal.CommandServices;

/// <summary>
/// Service implementation for handling room commands.
/// Orchestrates the flow between the repository and the domain logic.
/// </summary>
public class RoomCommandService(
    IRoomRepository roomRepository,
    IUnitOfWork unitOfWork)
    : IRoomCommandService
{
    public async Task<Room?> Handle(CreateRoomCommand command)
    {
        var room = new Room(command);
        await roomRepository.AddAsync(room);
        await unitOfWork.CompleteAsync();
        return room;
    }

    public async Task<Room?> Handle(UpdateRoomCommand command)
    {
        var room = await roomRepository.FindByIdAsync(command.Id);
        if (room is null) return null;

        // Apply domain updates
        room.UpdateInformation(
            command.RoomTypeId,
            command.Price,
            command.Description,
            command.Amenities
        );

        roomRepository.Update(room);
        await unitOfWork.CompleteAsync();
        return room;
    }

public async Task<Room?> Handle(DeleteRoomCommand command)
{
    // Find the room by its identifier
    var room = await roomRepository.FindByIdAsync(command.Id);

    // Return null if the room does not exist
    if (room is null) return null;

    // Remove the room from the repository
    roomRepository.Remove(room);

    // Save the changes to the database
    await unitOfWork.CompleteAsync();

    // Return the deleted room
    return room;
}
}
