using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;

public class BookingRepository(AppDbContext context) : BaseRepository<Booking>(context), IBookingRepository
{
    // Same statuses as BookingStatusExtensions.IsActive, in a form EF Core translates to SQL.
    private static readonly BookingStatus[] ActiveStatuses =
        [BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.CheckedIn];

    public async Task<IEnumerable<Booking>> FindByOwnerAsync(int userId, Guid? guestProfileId)
    {
        var guestId = new GuestId(userId);
        return await Context.Set<Booking>()
            .Where(b => b.GuestId == guestId || (guestProfileId != null && b.GuestProfileId == guestProfileId))
            .OrderByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> ListNewestFirstAsync(int? hotelId) =>
        await Context.Set<Booking>()
            .Where(b => hotelId == null || b.HotelId == hotelId)
            .OrderByDescending(b => b.Id)
            .ToListAsync();

    public async Task<IEnumerable<Booking>> FindByRoomIdAsync(int roomId)
    {
        return await Context.Set<Booking>()
            .Where(b => b.RoomId == roomId)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlySet<int>> FindRoomIdsWithActiveBookingOverlappingAsync(IReadOnlyCollection<int> roomIds, DateRange dates)
    {
        var booked = await Context.Set<Booking>()
            .Where(b => roomIds.Contains(b.RoomId)
                        && ActiveStatuses.Contains(b.Status)
                        && b.CheckInDate < dates.CheckOut
                        && dates.CheckIn < b.CheckOutDate)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();
        return booked.ToHashSet();
    }

    public async Task<bool> ExistsActiveBookingOverlappingAsync(int roomId, DateRange dates, int? excludingBookingId = null)
    {
        // Same predicate as DateRange.Overlaps, translated to SQL. Stored dates are calendar dates.
        return await Context.Set<Booking>().AnyAsync(b =>
            b.RoomId == roomId
            && (excludingBookingId == null || b.Id != excludingBookingId)
            && ActiveStatuses.Contains(b.Status)
            && b.CheckInDate < dates.CheckOut
            && dates.CheckIn < b.CheckOutDate);
    }

    public async Task<IReadOnlyList<Booking>> ListActiveOverlappingAsync(int? hotelId, DateRange window) =>
        await Context.Set<Booking>()
            .Where(b => (hotelId == null || b.HotelId == hotelId)
                        && ActiveStatuses.Contains(b.Status)
                        && b.CheckInDate < window.CheckOut
                        && window.CheckIn < b.CheckOutDate)
            .OrderBy(b => b.CheckInDate).ThenBy(b => b.RoomId)
            .ToListAsync();

    public async Task<IReadOnlyList<Booking>> ListPendingPaymentDueAsync(DateTimeOffset now) =>
        await Context.Set<Booking>()
            .Where(b => b.Status == BookingStatus.Pending && b.PaymentDueAt != null && b.PaymentDueAt <= now)
            .ToListAsync();
}
