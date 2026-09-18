using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Exceptions;

/// <summary>US-08: a guest can only move into an Available room (not being cleaned, repaired or occupied).</summary>
public class RoomNotReadyForCheckInException(int roomId, RoomStatus status)
    : BusinessRuleViolationException(
        $"Room {roomId} is not ready yet ({status}). Try again in a few minutes or request assistance from the front desk.");
