using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Repositories;

public class DigitalCheckInRepository(AppDbContext context) : IDigitalCheckInRepository
{
    public async Task AddAsync(DigitalCheckIn checkIn) => await context.Set<DigitalCheckIn>().AddAsync(checkIn);

    public Task<DigitalCheckIn?> FindByBookingIdAsync(int bookingId) =>
        context.Set<DigitalCheckIn>().FirstOrDefaultAsync(checkIn => checkIn.BookingId == bookingId);
}
