using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Application.Internal.Configuration;

/// <summary>Room operations (section <c>Rooms</c>, env vars <c>Rooms__*</c>). Validated at startup.</summary>
public class RoomOperationsSettings
{
    public const string SectionName = "Rooms";

    /// <summary>Hours a room may stay under maintenance before its hotel administrators are alerted (US-06: 24).</summary>
    [Range(1, 24 * 30)]
    public int MaintenanceAlertAfterHours { get; set; } = 24;

    public TimeSpan MaintenanceAlertAfter => TimeSpan.FromHours(MaintenanceAlertAfterHours);
}
