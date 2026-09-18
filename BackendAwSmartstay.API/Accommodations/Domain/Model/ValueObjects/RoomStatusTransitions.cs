namespace BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

/// <summary>
///     Valid room status transitions (US-29):
///     <list type="bullet">
///         <item>Available → Occupied (check-in), Cleaning, Maintenance;</item>
///         <item>Occupied → Cleaning (check-out), Maintenance: an occupied room is never available again before it is cleaned;</item>
///         <item>Cleaning → Available, Maintenance;</item>
///         <item>Maintenance → Available, Cleaning.</item>
///     </list>
/// </summary>
public static class RoomStatusTransitions
{
    private static readonly IReadOnlyDictionary<RoomStatus, RoomStatus[]> Allowed = new Dictionary<RoomStatus, RoomStatus[]>
    {
        [RoomStatus.Available] = [RoomStatus.Occupied, RoomStatus.Cleaning, RoomStatus.Maintenance],
        [RoomStatus.Occupied] = [RoomStatus.Cleaning, RoomStatus.Maintenance],
        [RoomStatus.Cleaning] = [RoomStatus.Available, RoomStatus.Maintenance],
        [RoomStatus.Maintenance] = [RoomStatus.Available, RoomStatus.Cleaning]
    };

    public static IReadOnlyList<RoomStatus> AllowedFrom(RoomStatus from) => Allowed[from];

    public static bool IsAllowed(RoomStatus from, RoomStatus to) => from == to || Allowed[from].Contains(to);
}
