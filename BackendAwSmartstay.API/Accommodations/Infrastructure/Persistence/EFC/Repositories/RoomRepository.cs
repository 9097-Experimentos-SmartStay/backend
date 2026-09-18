using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Accommodations.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Repository for Room aggregates.
/// </summary>
public class RoomRepository(AppDbContext context) : BaseRepository<Room>(context), IRoomRepository
{
    public override async Task<Room?> FindByIdAsync(int id)
    {
        return await Context.Set<Room>()
            .Include(r => r.RoomType) 
            .Include(r => r.Hotel) 
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> FindByIdForUpdateAsync(int id)
    {
        if (!Context.Database.IsMySql())
            return await FindByIdAsync(id); // Providers without row locks (tests): no concurrency to guard.

        // Row lock held until commit/rollback: a second booking of this room waits here.
        return await Context.Set<Room>()
            .FromSqlInterpolated($"SELECT * FROM rooms WHERE id = {id} FOR UPDATE")
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<Room>> FindOfferedForBookingAsync(int? hotelId)
    {
        return await Context.Set<Room>()
            .Include(r => r.RoomType)
            .Where(r => r.Status != RoomStatus.Maintenance && (hotelId == null || r.HotelId == hotelId))
            .OrderBy(r => r.HotelId).ThenBy(r => r.Price).ThenBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> ListByHotelAsync(int hotelId) =>
        await Context.Set<Room>().Include(r => r.RoomType).Where(r => r.HotelId == hotelId).OrderBy(r => r.Id).ToListAsync();

    public async Task<IReadOnlyList<Room>> ListInMaintenanceAsync() =>
        await Context.Set<Room>().Where(r => r.Status == RoomStatus.Maintenance).ToListAsync();

    public override async Task<IEnumerable<Room>> ListAsync()
    {
        return await Context.Set<Room>()
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
            .ToListAsync();
    }
}