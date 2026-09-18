namespace BackendAwSmartstay.API.Bookings.Interfaces.ACL;

/// <summary>
///     Read-only view of the reservations of rooms, for the Accommodations context (e.g. a room with active bookings
///     cannot be deleted). Separate from <see cref="IBookingsContextFacade"/> so it depends on nothing of
///     Accommodations (no dependency cycle between the two contexts).
/// </summary>
public interface IRoomReservationsFacade
{
    /// <summary>How many active bookings (Pending, Confirmed, CheckedIn) hold each of <paramref name="roomIds"/>.</summary>
    Task<IReadOnlyDictionary<int, int>> CountActiveBookingsAsync(IReadOnlyCollection<int> roomIds);
}
