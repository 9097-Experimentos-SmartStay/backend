namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to update an existing room aggregate.
/// Contains only the mutable fields.
/// </summary>
/// <param name="Id">The unique identifier of the room to update.</param>
/// <param name="RoomTypeId">The new room type ID.</param>
/// <param name="Price">The new price.</param>
/// <param name="Description">The new description.</param>
/// <param name="Amenities">The new list of amenities.</param>
public record UpdateRoomCommand(
    int Id,
    int RoomTypeId,
    decimal Price,
    string Description,
    List<string> Amenities
);