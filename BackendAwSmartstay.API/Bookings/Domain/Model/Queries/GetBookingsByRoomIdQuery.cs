namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
/// Query to retrieve bookings by room identifier.
/// </summary>
/// <param name="RoomId">The identifier of the room.</param>
public record GetBookingsByRoomIdQuery(int RoomId);
