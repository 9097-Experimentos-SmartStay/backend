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
        builder.Entity<Payment>().Property(p => p.TransactionId).IsRequired();
        
        builder.Entity<Payment>().Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}