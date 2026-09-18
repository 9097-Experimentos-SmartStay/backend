using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
/// Query to retrieve the bookings of a room visible to the requester (staff of its hotel).
/// </summary>
/// <param name="RoomId">The identifier of the room.</param>
/// <param name="Requester">Who asks.</param>
public record GetBookingsByRoomIdQuery(int RoomId, BookingRequester Requester);
