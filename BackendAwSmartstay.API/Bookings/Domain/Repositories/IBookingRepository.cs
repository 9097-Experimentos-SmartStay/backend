using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Bookings.Domain.Repositories;

/// <summary>
/// Repository interface for managing Booking aggregates.
/// </summary>
public interface IBookingRepository : IBaseRepository<Booking>
{
    /// <summary>Bookings owned by a guest: created with their account or attached to their guest profile.</summary>
    Task<IEnumerable<Booking>> FindByOwnerAsync(int userId, Guid? guestProfileId);

    /// <summary>Every booking of a hotel (or of every hotel when null), newest first.</summary>
    Task<IEnumerable<Booking>> ListNewestFirstAsync(int? hotelId);

    /// <summary>Bookings of a given room.</summary>
    Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId);

    /// <summary>
    ///     True when an active booking (Pending, Confirmed or CheckedIn) of the room shares a night with
    ///     <paramref name="dates"/>, other than <paramref name="excludingBookingId"/>.
    /// </summary>
    Task<bool> ExistsActiveBookingOverlappingAsync(int roomId, DateRange dates, int? excludingBookingId = null);

    /// <summary>Which of <paramref name="roomIds"/> have an active booking that shares a night with <paramref name="dates"/>.</summary>
    Task<IReadOnlySet<int>> FindRoomIdsWithActiveBookingOverlappingAsync(IReadOnlyCollection<int> roomIds, DateRange dates);

    /// <summary>Active bookings of a hotel (or every hotel) sharing a night with <paramref name="window"/>, by check-in.</summary>
    Task<IReadOnlyList<Booking>> ListActiveOverlappingAsync(int? hotelId, DateRange window);

    /// <summary>Active bookings (Pending, Confirmed, CheckedIn) per room, for the given rooms (rooms without any are omitted).</summary>
    Task<IReadOnlyDictionary<int, int>> CountActiveByRoomAsync(IReadOnlyCollection<int> roomIds);

    /// <summary>Pending bookings whose payment deadline is at or before <paramref name="now"/>.</summary>
    Task<IReadOnlyList<Booking>> ListPendingPaymentDueAsync(DateTimeOffset now);
}
