namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

/// <summary>
/// Query to retrieve all rooms for a specific hotel.
/// </summary>
/// <param name="RoomTypeId"></param>
public record GetRoomTypeByIdQuery(int RoomTypeId);

