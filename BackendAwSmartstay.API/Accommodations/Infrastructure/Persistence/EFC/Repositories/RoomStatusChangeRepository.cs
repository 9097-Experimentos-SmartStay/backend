using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Accommodations.Infrastructure.Persistence.EFC.Repositories;

public class RoomStatusChangeRepository(AppDbContext context) : IRoomStatusChangeRepository
{
    public async Task AddAsync(RoomStatusChange change) => await context.Set<RoomStatusChange>().AddAsync(change);

    public async Task<IReadOnlyList<RoomStatusChange>> ListByRoomAsync(int roomId, int limit) =>
        await context.Set<RoomStatusChange>()
            .Where(change => change.RoomId == roomId)
            .OrderByDescending(change => change.ChangedAt).ThenByDescending(change => change.Id)
            .Take(limit)
            .ToListAsync();
}
