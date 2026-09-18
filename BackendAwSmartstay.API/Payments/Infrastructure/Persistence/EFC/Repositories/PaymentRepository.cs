using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Payments.Infrastructure.Persistence.EFC.Repositories;

public class PaymentRepository(AppDbContext context) : BaseRepository<Payment>(context), IPaymentRepository
{
    public async Task<Payment?> FindByBookingIdAsync(int bookingId)
    {
        return await Context.Set<Payment>()
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Refunded)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsCompletedForBookingAsync(int bookingId)
    {
        return await Context.Set<Payment>()
            .AnyAsync(p => p.BookingId == bookingId && p.Status == PaymentStatus.Completed);
    }

    public Task<Payment?> FindCompletedByBookingIdAsync(int bookingId) =>
        Context.Set<Payment>().FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Status == PaymentStatus.Completed);
}
