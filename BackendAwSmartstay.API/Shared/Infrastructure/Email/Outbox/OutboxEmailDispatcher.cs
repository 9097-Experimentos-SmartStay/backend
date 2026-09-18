using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Outbox;

/// <summary>
///     Delivers the due <see cref="OutboxEmail"/>s through the configured <see cref="IEmailTransport"/>.
/// </summary>
/// <remarks>
///     <para>
///         <b>Claim.</b> A short transaction selects a batch of due rows with <c>FOR UPDATE SKIP LOCKED</c> (rows
///         locked by another instance are skipped, not waited for), counts the attempt and moves
///         <c>next_attempt_at</c> to the end of a lease, then commits. The mail system is called outside any
///         transaction, so no row lock is held during the HTTP/SMTP call. If the process dies mid-delivery the lease
///         expires and the e-mail becomes due again.
///     </para>
///     <para>
///         <b>Outcome.</b> Every state change is an update guarded by <c>status = 'Pending'</c> and the claimed attempt
///         number, so a late instance whose lease was taken over cannot overwrite the outcome of another. Success →
///         <c>Sent</c>; transient failure → retry with exponential backoff and jitter; permanent rejection or last
///         attempt → <c>Failed</c>. Delivery is at-least-once: only a crash between the provider's acceptance and the
///         <c>Sent</c> update can repeat an e-mail.
///     </para>
/// </remarks>
public class OutboxEmailDispatcher(
    AppDbContext context,
    IEmailTransport transport,
    IOptions<EmailSettings> options,
    TimeProvider timeProvider,
    ILogger<OutboxEmailDispatcher> logger) : IEmailDispatcher
{
    /// <summary>Batches per call: drains a backlog while keeping one call (e.g. the cron request) bounded.</summary>
    private const int MaxBatchesPerCall = 10;

    private EmailSettings.OutboxSettings Settings => options.Value.Outbox;

    public async Task<EmailDispatchReport> DispatchDueAsync(CancellationToken cancellationToken = default)
    {
        var report = EmailDispatchReport.Empty;
        for (var batch = 0; batch < MaxBatchesPerCall; batch++)
        {
            var claimed = await ClaimDueAsync(cancellationToken);
            foreach (var email in claimed)
                report += await DeliverAsync(email, cancellationToken);
            if (claimed.Count < Settings.BatchSize) break;
        }
        return report;
    }

    private async Task<IReadOnlyList<OutboxEmail>> ClaimDueAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var leaseEnd = now.AddSeconds(Settings.LeaseSeconds);
        var pending = nameof(OutboxEmailStatus.Pending);
        var emails = context.Set<OutboxEmail>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var ids = await context.Database.SqlQuery<Guid>($"""
            SELECT id AS Value FROM outbox_emails
            WHERE status = {pending} AND next_attempt_at <= {now}
            ORDER BY next_attempt_at
            LIMIT {Settings.BatchSize}
            FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);
        if (ids.Count == 0) return [];

        await emails.Where(e => ids.Contains(e.Id))
            .ExecuteUpdateAsync(set => set
                .SetProperty(e => e.Attempts, e => e.Attempts + 1)
                .SetProperty(e => e.NextAttemptAt, leaseEnd), cancellationToken);
        var claimed = await emails.AsNoTracking().Where(e => ids.Contains(e.Id))
            .OrderBy(e => e.CreatedAt).ToListAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return claimed;
    }

    private async Task<EmailDispatchReport> DeliverAsync(OutboxEmail email, CancellationToken cancellationToken)
    {
        try
        {
            await transport.DeliverAsync(email.ToMessage(), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down: give the attempt back so the e-mail is due again at once (next start or instance).
            await ClaimOf(email).ExecuteUpdateAsync(set => set
                .SetProperty(e => e.Attempts, email.Attempts - 1)
                .SetProperty(e => e.NextAttemptAt, timeProvider.GetUtcNow()), CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            return await RecordFailureAsync(email, exception, cancellationToken);
        }

        var now = timeProvider.GetUtcNow();
        var updated = await ClaimOf(email).ExecuteUpdateAsync(set => set
            .SetProperty(e => e.Status, OutboxEmailStatus.Sent)
            .SetProperty(e => e.SentAt, now)
            .SetProperty(e => e.LastError, (string?)null), CancellationToken.None);
        if (updated == 0)
            logger.LogWarning("E-mail {EmailId} was sent but its claim had expired; it may be sent twice.", email.Id);

        logger.LogInformation("E-mail '{Subject}' sent to {Recipient}.", email.Subject, EmailAddressMask.Mask(email.Recipient));
        return new EmailDispatchReport(1, 0, 0);
    }

    private async Task<EmailDispatchReport> RecordFailureAsync(OutboxEmail email, Exception exception, CancellationToken cancellationToken)
    {
        var permanent = exception is EmailDeliveryException { IsPermanent: true };
        var error = OutboxEmail.TruncateError(exception is EmailDeliveryException
            ? exception.Message
            : $"{exception.GetType().Name}: {exception.Message}");
        var recipient = EmailAddressMask.Mask(email.Recipient);

        if (permanent || email.Attempts >= Settings.MaxAttempts)
        {
            await ClaimOf(email).ExecuteUpdateAsync(set => set
                .SetProperty(e => e.Status, OutboxEmailStatus.Failed)
                .SetProperty(e => e.LastError, error), cancellationToken);
            logger.LogError("E-mail '{Subject}' to {Recipient} could not be delivered after {Attempts} attempt(s); giving up. {Error}",
                email.Subject, recipient, email.Attempts, error);
            return new EmailDispatchReport(0, 0, 1);
        }

        var nextAttemptAt = timeProvider.GetUtcNow() + RetryDelay(email.Attempts);
        await ClaimOf(email).ExecuteUpdateAsync(set => set
            .SetProperty(e => e.NextAttemptAt, nextAttemptAt)
            .SetProperty(e => e.LastError, error), cancellationToken);
        logger.LogWarning("E-mail '{Subject}' to {Recipient} failed (attempt {Attempt} of {MaxAttempts}); next attempt at {NextAttemptAt:u}. {Error}",
            email.Subject, recipient, email.Attempts, Settings.MaxAttempts, nextAttemptAt, error);
        return new EmailDispatchReport(0, 1, 0);
    }

    /// <summary>The row as this dispatcher claimed it: still pending, at the attempt it counted.</summary>
    private IQueryable<OutboxEmail> ClaimOf(OutboxEmail email) =>
        context.Set<OutboxEmail>().Where(e =>
            e.Id == email.Id && e.Status == OutboxEmailStatus.Pending && e.Attempts == email.Attempts);

    /// <summary>Exponential backoff: initial delay × 2^(attempt − 1), capped, ±20 % jitter (instances do not retry in lockstep).</summary>
    private TimeSpan RetryDelay(int attempt)
    {
        var initial = TimeSpan.FromSeconds(Settings.InitialRetryDelaySeconds);
        var cap = TimeSpan.FromMinutes(Settings.MaxRetryDelayMinutes);
        var exponential = initial * Math.Pow(2, Math.Min(attempt - 1, 20));
        var delay = exponential < cap ? exponential : cap;
        return delay * (0.8 + Random.Shared.NextDouble() * 0.4);
    }
}
