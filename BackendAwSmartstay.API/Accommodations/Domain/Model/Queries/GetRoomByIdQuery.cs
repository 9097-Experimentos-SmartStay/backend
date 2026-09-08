namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

/// <summary>
/// Query to retrieve all rooms for a specific hotel.
/// </summary>
/// <param name="RoomId"></param>
public record GetRoomByIdQuery(int RoomId);

