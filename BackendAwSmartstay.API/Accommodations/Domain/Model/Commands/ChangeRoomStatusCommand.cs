using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>Change the operational status of a room (US-29 scenario 2).</summary>
/// <param name="RoomId">The room.</param>
/// <param name="Status">The new status.</param>
public record ChangeRoomStatusCommand(int RoomId, RoomStatus Status);
