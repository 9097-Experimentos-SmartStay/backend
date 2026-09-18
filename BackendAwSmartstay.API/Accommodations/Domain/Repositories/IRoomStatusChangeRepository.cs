using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;

namespace BackendAwSmartstay.API.Accommodations.Domain.Repositories;

/// <summary>Status history of the rooms (US-06 scenario 3). Append-only.</summary>
public interface IRoomStatusChangeRepository
{
    Task AddAsync(RoomStatusChange change);

    /// <summary>The changes of a room, newest first.</summary>
    Task<IReadOnlyList<RoomStatusChange>> ListByRoomAsync(int roomId, int limit);
}
