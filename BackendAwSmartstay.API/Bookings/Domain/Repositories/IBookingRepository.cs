using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Bookings.Domain.Repositories;

/// <summary>
/// Repository interface for managing Booking aggregates.
/// </summary>
public interface IBookingRepository : IBaseRepository<Booking>
{
}
