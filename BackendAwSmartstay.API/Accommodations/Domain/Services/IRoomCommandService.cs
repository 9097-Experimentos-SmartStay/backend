using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

/// <summary>
/// Defines the contract for services that handle room state changes.
/// </summary>
public interface IRoomCommandService
{
    Task<Room?> Handle(CreateRoomCommand command);
    
    /// <summary>
    /// Handles the update of a room.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <returns>The updated room or null if not found.</returns>
    Task<Room?> Handle(UpdateRoomCommand command);

    /// <summary>
    /// Handles the deletion of a room.
    /// </summary>
    /// <param name="command">The delete command.</param>
    /// <returns>The deleted room or null if not found.</returns>
    Task<Room?> Handle(DeleteRoomCommand command);

    /// <summary>Changes the room status (US-29). Null when the room does not exist.</summary>
    Task<Room?> Handle(ChangeRoomStatusCommand command);

    /// <summary>US-08: the room of a completed check-in becomes Occupied (must be Available).</summary>
    Task<Room> Handle(OccupyRoomForCheckInCommand command);

    /// <returns>How many rooms were alerted.</returns>
    Task<int> Handle(RaiseMaintenanceAlertsCommand command);
}