using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for updating an existing room.
/// Represents the data payload received from the client.
/// </summary>
public record UpdateRoomResource(
    [Required] int RoomTypeId,
    [Required] decimal Price,
    [Required] string Description,
    List<string> Amenities
);