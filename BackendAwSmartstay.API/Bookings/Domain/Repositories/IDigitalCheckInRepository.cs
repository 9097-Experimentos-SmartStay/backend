using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Bookings.Domain.Repositories;

/// <summary>Digital check-ins (US-08).</summary>
public interface IDigitalCheckInRepository
{
    Task AddAsync(DigitalCheckIn checkIn);

    Task<DigitalCheckIn?> FindByBookingIdAsync(int bookingId);
}
