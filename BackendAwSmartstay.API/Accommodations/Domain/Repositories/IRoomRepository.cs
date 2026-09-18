using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Accommodations.Domain.Repositories;

/// <summary>
/// Repository interface for managing Room aggregates.
/// </summary>
public interface IRoomRepository : IBaseRepository<Room>
{
    /// <summary>
    ///     Loads the room and locks its row until the current transaction ends (<c>SELECT ... FOR UPDATE</c>), so
    ///     concurrent bookings of the same room are serialized (R1). Must run inside a transaction.
    /// </summary>
    Task<Room?> FindByIdForUpdateAsync(int id);

    /// <summary>Rooms of <paramref name="hotelId"/> (or of every hotel) that are not under maintenance.</summary>
    Task<IEnumerable<Room>> FindOfferedForBookingAsync(int? hotelId);

    /// <summary>Every room of a hotel with its type, by id (US-06 room map).</summary>
    Task<IReadOnlyList<Room>> ListByHotelAsync(int hotelId);

    /// <summary>Rooms currently under maintenance (US-06 scenario 4).</summary>
    Task<IReadOnlyList<Room>> ListInMaintenanceAsync();
}
