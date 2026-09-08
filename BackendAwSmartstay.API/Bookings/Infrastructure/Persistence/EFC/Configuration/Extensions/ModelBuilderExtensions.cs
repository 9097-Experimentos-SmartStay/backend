using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyBookingsConfiguration(this ModelBuilder builder)
    {
        // Booking Entity
        builder.Entity<Booking>().HasKey(b => b.Id);
        builder.Entity<Booking>().Property(b => b.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Booking>().Property(b => b.RoomId).IsRequired();
        builder.Entity<Booking>().Property(b => b.GuestName).IsRequired().HasMaxLength(100);
        builder.Entity<Booking>().Property(b => b.GuestEmail).IsRequired().HasMaxLength(200);
        builder.Entity<Booking>().Property(b => b.CheckInDate).IsRequired();
        builder.Entity<Booking>().Property(b => b.CheckOutDate).IsRequired();
        builder.Entity<Booking>().Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}

