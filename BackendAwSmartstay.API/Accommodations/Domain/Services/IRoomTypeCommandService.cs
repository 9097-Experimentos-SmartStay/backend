using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

/// <summary>
/// Service interface to handle internal operations and commands related to room types.
/// </summary>

public interface IRoomTypeCommandService
{

    /// <summary>
    /// Processes the creation of a new room type category in the system.
    /// </summary>
    /// <param name="command">The command data containing the room type name and description.</param>
    /// <returns>
    /// A task containing the newly created <see cref="RoomType"/> entity, 
    /// or null if the creation process fails.
    /// </returns>
    public Task<RoomType?> Handle(CreateRoomTypeCommand command);
}

