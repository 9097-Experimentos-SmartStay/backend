using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Outbox;

public static class OutboxEmailModelBuilderExtensions
{
    /// <summary>Table <c>outbox_emails</c> of the transactional e-mail outbox.</summary>
    public static void ApplyEmailOutboxConfiguration(this ModelBuilder builder)
    {
        var entity = builder.Entity<OutboxEmail>();
        entity.ToTable("outbox_emails");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.Recipient).HasMaxLength(OutboxEmail.RecipientMaxLength).IsRequired();
        entity.Property(e => e.Subject).HasMaxLength(OutboxEmail.SubjectMaxLength).IsRequired();
        entity.Property(e => e.HtmlBody).HasColumnType("mediumtext").IsRequired();
        entity.Property(e => e.TextBody).HasColumnType("mediumtext").IsRequired();
        entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        entity.Property(e => e.Attempts).IsRequired();
        entity.Property(e => e.NextAttemptAt).IsRequired();
        entity.Property(e => e.LastError).HasMaxLength(OutboxEmail.LastErrorMaxLength);
        entity.Property(e => e.CreatedAt).IsRequired();
        // The dispatcher's claim: WHERE status = 'Pending' AND next_attempt_at <= now ORDER BY next_attempt_at.
        entity.HasIndex(e => new { e.Status, e.NextAttemptAt });
    }
}
