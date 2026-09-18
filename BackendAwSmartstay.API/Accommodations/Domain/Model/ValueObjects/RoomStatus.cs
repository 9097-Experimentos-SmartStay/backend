namespace BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

/// <summary>Operational status of a room (US-29).</summary>
public enum RoomStatus
{
    /// <summary>Clean and free: can be offered and occupied.</summary>
    Available,
    /// <summary>A guest is staying in it.</summary>
    Occupied,
    /// <summary>Being cleaned after a stay.</summary>
    Cleaning,
    /// <summary>Out of service: never offered for booking.</summary>
    Maintenance
}
