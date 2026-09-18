namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>A room on the map (US-06 scenario 2).</summary>
/// <param name="Id">Room id.</param>
/// <param name="Number">Room number (what the map shows).</param>
/// <param name="RoomTypeName">Room type.</param>
/// <param name="Description">Description (e.g. "Room 101 - Standard view.").</param>
/// <param name="Price">Price per night.</param>
/// <param name="Status">Available, Occupied, Cleaning or Maintenance.</param>
/// <param name="StatusSince">Since when it has this status (UTC).</param>
/// <param name="MaintenanceOverdue">Under maintenance for longer than the alert threshold.</param>
/// <param name="AllowedNextStatuses">Statuses it can change to (for quick changes from the map).</param>
public record RoomMapItemResource(int Id, string Number, string RoomTypeName, string Description, decimal Price, string Status,
    DateTimeOffset StatusSince, bool MaintenanceOverdue, IReadOnlyList<string> AllowedNextStatuses);

/// <summary>Every room of a hotel with its status and a count per status.</summary>
public record RoomMapResource(int HotelId, string HotelName, DateTimeOffset GeneratedAt,
    IReadOnlyDictionary<string, int> Summary, IReadOnlyList<RoomMapItemResource> Rooms);

/// <summary>One change of the status of a room (US-06 scenario 3).</summary>
/// <param name="Id">History line id.</param>
/// <param name="RoomId">The room.</param>
/// <param name="FromStatus">Previous status.</param>
/// <param name="ToStatus">New status.</param>
/// <param name="Origin">Staff (manual change) or CheckIn (digital check-in of a guest).</param>
/// <param name="ChangedAt">When (UTC).</param>
/// <param name="ChangedByUserId">Who.</param>
/// <param name="ChangedByEmail">Their e-mail at the time.</param>
public record RoomStatusChangeResource(long Id, int RoomId, string FromStatus, string ToStatus, string Origin,
    DateTimeOffset ChangedAt, int? ChangedByUserId, string? ChangedByEmail);

/// <summary>Result of the maintenance alert job.</summary>
/// <param name="Alerted">Rooms whose administrators were alerted in this run.</param>
public record MaintenanceAlertsResultResource(int Alerted);
