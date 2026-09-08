namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to create a new room type.
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CreateRoomTypeCommand(string Name, string Description);
