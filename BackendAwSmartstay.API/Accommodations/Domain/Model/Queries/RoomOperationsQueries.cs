namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

/// <summary>US-06 scenario 2: every room of a hotel with its status.</summary>
public record GetRoomMapQuery(int HotelId);

/// <summary>US-06 scenario 3: the status history of a room, newest first.</summary>
public record GetRoomStatusHistoryQuery(int RoomId, int Limit = 100);
