using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;

public class BookingRepository(AppDbContext context) : BaseRepository<Booking>(context), IBookingRepository
{
    public async Task<IEnumerable<Booking>> FindByOwnerAsync(int userId, Guid? guestProfileId)
    {
        var guestId = new GuestId(userId);
        return await Context.Set<Booking>()
            .Where(b => b.GuestId == guestId || (guestProfileId != null && b.GuestProfileId == guestProfileId))
            .OrderByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> ListNewestFirstAsync() =>
        await Context.Set<Booking>().OrderByDescending(b => b.Id).ToListAsync();

    public async Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId)
    {
        return await Context.Set<Booking>()
            .Where(b => b.RoomId == roomId)
            .ToListAsync();
    }

    public async Task<bool> ExistsActiveBookingOverlappingAsync(int roomId, DateRange dates)
    {
        // Same predicate as DateRange.Overlaps, translated to SQL. Stored dates are calendar dates.
        return await Context.Set<Booking>().AnyAsync(b =>
            b.RoomId == roomId
            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
            && b.CheckInDate < dates.CheckOut
            && dates.CheckIn < b.CheckOutDate);
    }
}
