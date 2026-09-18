using BackendAwSmartstay.API.Audit.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;

/// <summary>
///     One line of the access audit log (US-03 scenario 4: date, time, user and action). Immutable once recorded.
/// </summary>
public class AuditEntry
{
    public const int MaxDetailsLength = 500;

    /// <summary>EF Core constructor.</summary>
    protected AuditEntry() { }

    private AuditEntry(DateTimeOffset occurredAt, AuditAction action, AuditOutcome outcome,
        int? actorUserId, string? actorEmail, int? targetUserId, string? targetEmail, int? hotelId,
        string? ipAddress, string? details)
    {
        OccurredAt = occurredAt;
        Action = action;
        Outcome = outcome;
        ActorUserId = actorUserId;
        ActorEmail = actorEmail;
        TargetUserId = targetUserId;
        TargetEmail = targetEmail;
        HotelId = hotelId;
        IpAddress = ipAddress;
        Details = details;
    }

    public long Id { get; private set; }

    /// <summary>When the action happened (UTC).</summary>
    public DateTimeOffset OccurredAt { get; private set; }

    public AuditAction Action { get; private set; }
    public AuditOutcome Outcome { get; private set; }

    /// <summary>Who performed the action (null for an anonymous attempt with an unknown e-mail).</summary>
    public int? ActorUserId { get; private set; }
    public string? ActorEmail { get; private set; }

    /// <summary>The account the action was about.</summary>
    public int? TargetUserId { get; private set; }
    public string? TargetEmail { get; private set; }

    /// <summary>Hotel of the target account: decides which hotel administrator can read the entry.</summary>
    public int? HotelId { get; private set; }

    /// <summary>Client IP address of the request.</summary>
    public string? IpAddress { get; private set; }

    /// <summary>Additional facts (failure reason, role change...).</summary>
    public string? Details { get; private set; }

    public static AuditEntry Record(DateTimeOffset occurredAt, AuditAction action, AuditOutcome outcome,
        int? actorUserId, string? actorEmail, int? targetUserId, string? targetEmail, int? hotelId,
        string? ipAddress, string? details = null)
    {
        if (actorUserId is null && string.IsNullOrWhiteSpace(actorEmail))
            throw new DomainValidationException(AuditErrorCodes.InternalInvariant, "An audit entry must identify who acted (user id or e-mail).");
        if (details is { Length: > MaxDetailsLength })
            details = details[..MaxDetailsLength];

        return new AuditEntry(occurredAt.ToUniversalTime(), action, outcome, actorUserId, actorEmail, targetUserId,
            targetEmail, hotelId, ipAddress, details);
    }
}
