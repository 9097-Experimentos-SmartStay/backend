using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Payments.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyPaymentsConfiguration(this ModelBuilder builder)
    {
        // Payment Entity
        builder.Entity<Payment>().ToTable("payments");
        builder.Entity<Payment>().HasKey(p => p.Id);
        builder.Entity<Payment>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Payment>().Property(p => p.TransactionId).IsRequired().HasMaxLength(100);
        builder.Entity<Payment>().Property(p => p.Method).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Entity<Payment>().Property(p => p.Status).HasConversion<int>().IsRequired();
        builder.Entity<Payment>().Property(p => p.OperationNumber).HasMaxLength(Payment.MaxOperationNumberLength);
        builder.Entity<Payment>().Property(p => p.Note).HasMaxLength(Payment.MaxNoteLength);
        builder.Entity<Payment>().Property(p => p.FailureReason).HasMaxLength(200);
        builder.Entity<Payment>().HasIndex(p => p.BookingId);
        builder.Entity<Payment>().Ignore(p => p.DomainEvents);
        
        builder.Entity<Payment>().Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}