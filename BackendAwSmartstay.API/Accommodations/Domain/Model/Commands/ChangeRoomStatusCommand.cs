using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>A staff member changes the operational status of a room (US-06 scenario 1).</summary>
/// <param name="RoomId">The room.</param>
/// <param name="Status">The new status.</param>
/// <param name="ChangedByUserId">Who changes it.</param>
/// <param name="ChangedByEmail">Their e-mail (kept in the history).</param>
public record ChangeRoomStatusCommand(int RoomId, RoomStatus Status, int ChangedByUserId, string? ChangedByEmail);

/// <summary>US-08: the guest of a completed check-in moves into the room (it becomes Occupied).</summary>
public record OccupyRoomForCheckInCommand(int RoomId, int? GuestUserId, string? GuestEmail);

/// <summary>US-06 scenario 4: alert the administrators of rooms under maintenance for too long. Run by the scheduler.</summary>
public record RaiseMaintenanceAlertsCommand;
