using BackendAwSmartstay.API.Analytics.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Analytics.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates; // Acceso a BookingStatus
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Analytics.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Implementation of the analytics repository using Entity Framework Core.
/// </summary>
public class AnalyticsRepository(AppDbContext context) : IAnalyticsRepository
{
    public async Task<PerformanceMetrics> GetMonthlyMetricsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // 1. Calculate Revenue (From Payments table ideally, or Bookings logic)
        // Assuming we look at Payments for confirmed revenue
        var totalRevenue = await context.Set<Payments.Domain.Model.Aggregates.Payment>()
            .Where(p => p.PaymentDate >= startOfMonth && p.PaymentDate <= endOfMonth && p.Status == Payments.Domain.Model.Aggregates.PaymentStatus.Completed)
            .SumAsync(p => p.Amount);

        // 2. Booking Stats
        var bookingsQuery = context.Set<Booking>()
            .Where(b => b.CheckInDate >= startOfMonth && b.CheckInDate <= endOfMonth);

        var totalBookings = await bookingsQuery.CountAsync();
        
        var cancelledBookings = await bookingsQuery
            .CountAsync(b => b.Status == BookingStatus.Cancelled);

        // 3. Occupancy Rate (Simplified logic: Booked Rooms / Total Rooms * 100)
        // Note: For a real rigorous calculation, we'd check day-by-day availability.
        var totalRooms = await context.Set<Accommodations.Domain.Model.Aggregates.Room>().CountAsync();
        
        double occupancyRate = 0;
        if (totalRooms > 0 && totalBookings > 0)
        {
            // Simple heuristic: (Confirmed Bookings / Total Rooms) * 100
            // This is a snapshot, a real system would calculate room-nights.
            var activeBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Confirmed);
            occupancyRate = ((double)activeBookings / totalRooms) * 100;
        }

        return new PerformanceMetrics
        {
            TotalRevenue = totalRevenue,
            TotalBookings = totalBookings,
            CancelledBookings = cancelledBookings,
            OccupancyRate = Math.Round(occupancyRate, 2)
        };
    }
}