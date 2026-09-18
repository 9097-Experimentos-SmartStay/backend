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

    /// <summary>Every booking, newest first.</summary>
    Task<IEnumerable<Booking>> ListNewestFirstAsync();

    /// <summary>Bookings of a given room.</summary>
    Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId);

    /// <summary>True when an active booking (Pending or Confirmed) of the room shares a night with <paramref name="dates"/>.</summary>
    Task<bool> ExistsActiveBookingOverlappingAsync(int roomId, DateRange dates);

    /// <summary>Which of <paramref name="roomIds"/> have an active booking that shares a night with <paramref name="dates"/>.</summary>
    Task<IReadOnlySet<int>> FindRoomIdsWithActiveBookingOverlappingAsync(IReadOnlyCollection<int> roomIds, DateRange dates);
}
