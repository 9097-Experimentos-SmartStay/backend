using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        builder.Entity<User>().ToTable("users");

        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();

        builder.Entity<User>().Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(v => v.Value, v => new Username(v));

        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);

        builder.Entity<User>().Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(v => v.Value, v => new Role(v));

        builder.Entity<User>().Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserStatus.Active);

        // Indexes
        builder.Entity<User>().HasIndex(u => u.Username, "i_x_users_username").IsUnique();
        builder.Entity<User>().HasIndex(u => u.Status, "i_x_users_status");

        builder.Entity<User>().Property(u => u.HotelId)
            .HasColumnName("hotel_id")
            .IsRequired(false);

        builder.Entity<User>().Property(u => u.ChainId)
            .HasColumnName("chain_id")
            .IsRequired(false);

        builder.Entity<User>().Property(u => u.TokenVersion)
            .HasColumnName("token_version")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Entity<User>().Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
            .ValueGeneratedOnAdd();

        builder.Entity<User>().Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
            .ValueGeneratedOnAddOrUpdate();
    }
}