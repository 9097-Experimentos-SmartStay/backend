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

        builder.Entity<User>().Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(Email.MaxLength)
            .HasConversion(v => v.Value, v => Email.FromPersistence(v));

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
        builder.Entity<User>().HasIndex(u => u.Email, "i_x_users_email").IsUnique();
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

        builder.Entity<User>().Property(u => u.FirstName).HasMaxLength(PersonName.MaxLength);
        builder.Entity<User>().Property(u => u.LastName).HasMaxLength(PersonName.MaxLength);
        builder.Entity<User>().Property(u => u.EmailVerified).IsRequired().HasDefaultValue(false);
        builder.Entity<User>().Property(u => u.FailedSignInAttempts).IsRequired().HasDefaultValue(0);
        builder.Entity<User>().Ignore(u => u.DomainEvents);

        // Single-use links (e-mail verification, password reset). Only the hash is stored.
        builder.Entity<AccountToken>().ToTable("account_tokens");
        builder.Entity<AccountToken>().HasKey(t => t.Id);
        builder.Entity<AccountToken>().Property(t => t.Id).ValueGeneratedOnAdd();
        builder.Entity<AccountToken>().Property(t => t.Purpose).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Entity<AccountToken>().Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Entity<AccountToken>().HasIndex(t => t.TokenHash).IsUnique();
        builder.Entity<AccountToken>().HasIndex(t => new { t.UserId, t.Purpose });
        builder.Entity<AccountToken>().HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        // Remembered sessions (refresh tokens with rotation). Only the hash is stored.
        builder.Entity<RefreshToken>().ToTable("refresh_tokens");
        builder.Entity<RefreshToken>().HasKey(t => t.Id);
        builder.Entity<RefreshToken>().Property(t => t.Id).ValueGeneratedOnAdd();
        builder.Entity<RefreshToken>().Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Entity<RefreshToken>().Property(t => t.RevocationReason).HasConversion<string>().HasMaxLength(30);
        builder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
        builder.Entity<RefreshToken>().HasIndex(t => t.FamilyId);
        builder.Entity<RefreshToken>().HasIndex(t => t.UserId);
        builder.Entity<RefreshToken>().HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}