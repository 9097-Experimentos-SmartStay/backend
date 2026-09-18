using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Accommodations.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Repository for handling Hotel aggregate persistence.
/// Overrides standard behavior to include related Room data for pricing calculations.
/// </summary>
public class HotelRepository(AppDbContext context) : BaseRepository<Hotel>(context), IHotelRepository
{
    /// <summary>
    /// Retrieves all hotels including their room data to calculate base prices.
    /// </summary>
    /// <returns>List of hotels with rooms loaded.</returns>
    public override async Task<IEnumerable<Hotel>> ListAsync()
    {
        return await Context.Set<Hotel>()
            .Include(h => h.Rooms)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a hotel by ID including room data.
    /// </summary>
    /// <param name="id">The hotel ID.</param>
    /// <returns>The hotel with rooms loaded.</returns>
    public override async Task<Hotel?> FindByIdAsync(int id)
    {
        return await Context.Set<Hotel>()
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<bool> ExistsByHostIdAsync(int hostId) =>
        await Context.Set<Hotel>().AnyAsync(h => h.HostId == hostId);

    public async Task<bool> AcceptsBookingsAsync(int hotelId) =>
        await Context.Set<Hotel>().AnyAsync(h => h.Id == hotelId && h.PaymentSettings != null);

    public async Task<HotelPaymentSettings?> FindPaymentSettingsAsync(int hotelId) =>
        await Context.Set<Hotel>().AsNoTracking()
            .Where(h => h.Id == hotelId)
            .Select(h => h.PaymentSettings)
            .FirstOrDefaultAsync();
}