namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to delete an existing room aggregate.
/// </summary>
/// <param name="Id">The unique identifier of the room to delete.</param>
public record DeleteRoomCommand(int Id);