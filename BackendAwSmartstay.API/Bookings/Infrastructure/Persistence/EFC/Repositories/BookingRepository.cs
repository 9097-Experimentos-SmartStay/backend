using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;

public class BookingRepository(AppDbContext context) : BaseRepository<Booking>(context), IBookingRepository
{
    public async Task<IEnumerable<Booking>> FindByOwnerAsync(int userId, Guid? guestProfileId)
    {
        return await Context.Set<Booking>()
            .Where(b => b.UserId == userId || (guestProfileId != null && b.GuestProfileId == guestProfileId))
            .OrderByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId)
    {
        return await Context.Set<Booking>()
            .Where(b => b.RoomId == roomId)
            .ToListAsync();
    }
}
