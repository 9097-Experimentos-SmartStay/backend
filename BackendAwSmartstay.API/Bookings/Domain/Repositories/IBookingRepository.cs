using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Bookings.Domain.Repositories;

/// <summary>
/// Repository interface for managing Booking aggregates.
/// </summary>
public interface IBookingRepository : IBaseRepository<Booking>
{
    /// <summary>Bookings owned by a guest: created by the user or attached to their guest profile.</summary>
    Task<IEnumerable<Booking>> FindByOwnerAsync(int userId, Guid? guestProfileId);

    /// <summary>Bookings of a given room.</summary>
    Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId);
}
