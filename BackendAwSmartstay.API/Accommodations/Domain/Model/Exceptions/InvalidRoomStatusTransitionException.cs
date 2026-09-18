using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;

/// <summary>The requested status change is not a valid transition from the current status.</summary>
public class InvalidRoomStatusTransitionException(int roomId, RoomStatus from, RoomStatus to)
    : BusinessRuleViolationException(
        $"Room {roomId} cannot change from {from} to {to}. Allowed from {from}: {string.Join(", ", RoomStatusTransitions.AllowedFrom(from))}.");
