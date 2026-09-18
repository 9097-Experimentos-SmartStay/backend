using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
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
        builder.Entity<Booking>().Property(b => b.GuestProfileId)
            .HasColumnName("guest_profile_id")
            .IsRequired(false);
        // The guest reference is the IAM user id (column kept as user_id; exposed as userId in the API).
        builder.Entity<Booking>().Property(b => b.GuestId)
            .HasColumnName("user_id")
            .HasConversion(guestId => guestId!.Value, value => new GuestId(value))
            .IsRequired(false);
        builder.Entity<Booking>().HasIndex(b => b.GuestId);
        builder.Entity<Booking>().Ignore(b => b.Dates);
        builder.Entity<Booking>().Ignore(b => b.Nights);
        builder.Entity<Booking>().Ignore(b => b.CanBePaid);
        builder.Entity<Booking>().Ignore(b => b.TotalPrice);
        builder.Entity<Booking>().Ignore(b => b.DomainEvents);
        builder.Entity<Booking>().Property(b => b.Code)
            .HasMaxLength(BookingCode.MaxLength)
            .HasConversion(code => code.Value, value => new BookingCode(value))
            .IsRequired();
        builder.Entity<Booking>().HasIndex(b => b.Code).IsUnique();
        builder.Entity<Booking>().Property(b => b.HotelId).IsRequired();
        builder.Entity<Booking>().HasIndex(b => new { b.HotelId, b.CheckInDate });
        builder.Entity<Booking>().Property(b => b.GuestPhone).HasMaxLength(20);
        builder.Entity<Booking>().Property(b => b.PricePerNight).HasColumnType("decimal(18,2)").IsRequired();
        builder.Entity<Booking>().Property(b => b.CancellationReason).HasConversion<string>().HasMaxLength(30);
        builder.Entity<Booking>().HasIndex(b => new { b.Status, b.PaymentDueAt });
        builder.Entity<Booking>().Property(b => b.GuestName).IsRequired().HasMaxLength(GuestContact.MaxNameLength);
        builder.Entity<Booking>().Property(b => b.GuestEmail).IsRequired().HasMaxLength(200);
        builder.Entity<Booking>().Property(b => b.CheckInDate).IsRequired();
        builder.Entity<Booking>().Property(b => b.CheckOutDate).IsRequired();
        builder.Entity<Booking>().Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}

