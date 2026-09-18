using System.Text.Json;
using System.Text.Json.Serialization;
using BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Audit.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    private const int MaxDetailsLength = 500;

    private static readonly JsonSerializerOptions DetailsJson = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void ApplyAuditConfiguration(this ModelBuilder builder)
    {
        builder.Entity<AuditEntry>().ToTable("audit_entries");
        builder.Entity<AuditEntry>().HasKey(e => e.Id);
        builder.Entity<AuditEntry>().Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Entity<AuditEntry>().Property(e => e.Action).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Entity<AuditEntry>().Property(e => e.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Entity<AuditEntry>().Property(e => e.ActorEmail).HasMaxLength(254);
        builder.Entity<AuditEntry>().Property(e => e.TargetEmail).HasMaxLength(254);
        builder.Entity<AuditEntry>().Property(e => e.IpAddress).HasMaxLength(45);
        // Structured facts stored as a small JSON object in the same column (camelCase, nulls omitted).
        builder.Entity<AuditEntry>().Property(e => e.Details)
            .HasConversion(
                details => details == null ? null : JsonSerializer.Serialize(details, DetailsJson),
                json => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<AuditDetails>(json, DetailsJson))
            .HasMaxLength(MaxDetailsLength);
        builder.Entity<AuditEntry>().HasIndex(e => e.OccurredAt);
        builder.Entity<AuditEntry>().HasIndex(e => new { e.HotelId, e.OccurredAt });
        builder.Entity<AuditEntry>().HasIndex(e => e.ActorUserId);
        builder.Entity<AuditEntry>().HasIndex(e => e.TargetUserId);
    }
}
