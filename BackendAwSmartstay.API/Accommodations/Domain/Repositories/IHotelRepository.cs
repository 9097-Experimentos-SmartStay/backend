using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using ValueObjects = BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Accommodations.Domain.Repositories;

/// <summary>
/// Repository interface for managing Hotel aggregates.
/// </summary>
public interface IHotelRepository : IBaseRepository<Hotel>
{
    /// <summary>True when a hotel hosted by the given user exists.</summary>
    Task<bool> ExistsByHostIdAsync(int hostId);

    /// <summary>True when the hotel exists and has payment methods (it accepts bookings).</summary>
    Task<bool> AcceptsBookingsAsync(int hotelId);

    /// <summary>The payment methods of the hotel, or null when the hotel does not exist or has none (read-only).</summary>
    Task<ValueObjects.HotelPaymentSettings?> FindPaymentSettingsAsync(int hotelId);
}