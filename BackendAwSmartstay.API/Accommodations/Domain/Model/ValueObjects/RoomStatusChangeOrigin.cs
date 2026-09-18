namespace BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

/// <summary>What changed the status of a room.</summary>
public enum RoomStatusChangeOrigin
{
    /// <summary>A staff member changed it (US-06 scenario 1).</summary>
    Staff,
    /// <summary>A guest completed the digital check-in (US-08): the room became Occupied.</summary>
    CheckIn
}
