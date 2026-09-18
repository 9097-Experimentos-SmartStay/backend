using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Outbox;

/// <summary>Delivery state of an <see cref="OutboxEmail"/>.</summary>
public enum OutboxEmailStatus
{
    /// <summary>Waiting for (another) delivery attempt at <see cref="OutboxEmail.NextAttemptAt"/>.</summary>
    Pending,

    /// <summary>Accepted by the mail system. Final.</summary>
    Sent,

    /// <summary>Given up: permanent rejection or too many attempts. Final.</summary>
    Failed
}

/// <summary>
///     An outgoing e-mail of the transactional outbox (table <c>outbox_emails</c>). It is stored in the same
///     transaction as the change that caused it, so it exists if and only if that change was committed, and it
///     survives restarts until the dispatcher delivers it.
/// </summary>
/// <remarks>
///     Delivery state changes are applied by <see cref="OutboxEmailDispatcher"/> with set-based updates guarded by
///     the status and the attempt count it claimed (several instances never deliver the same claim twice).
/// </remarks>
public class OutboxEmail
{
    public const int RecipientMaxLength = 320;
    public const int SubjectMaxLength = 255;
    public const int LastErrorMaxLength = 500;

    private OutboxEmail()
    {
    }

    public Guid Id { get; private set; }

    public string Recipient { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string HtmlBody { get; private set; } = string.Empty;

    public string TextBody { get; private set; } = string.Empty;

    public OutboxEmailStatus Status { get; private set; }

    /// <summary>Delivery attempts started so far (counted when the e-mail is claimed).</summary>
    public int Attempts { get; private set; }

    /// <summary>When the e-mail is due; while it is claimed, the end of the claim's lease.</summary>
    public DateTimeOffset NextAttemptAt { get; private set; }

    /// <summary>Short, secret-free reason of the last failed attempt.</summary>
    public string? LastError { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>A new e-mail, due at once.</summary>
    public static OutboxEmail Enqueue(EmailMessage message, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message.To);
        if (message.To.Length > RecipientMaxLength)
            throw new ArgumentException($"The recipient exceeds {RecipientMaxLength} characters.", nameof(message));

        return new OutboxEmail
        {
            Id = Guid.NewGuid(),
            Recipient = message.To.Trim(),
            Subject = message.Subject.Length > SubjectMaxLength ? message.Subject[..SubjectMaxLength] : message.Subject,
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
            Status = OutboxEmailStatus.Pending,
            Attempts = 0,
            NextAttemptAt = now,
            CreatedAt = now
        };
    }

    public EmailMessage ToMessage() => new(Recipient, Subject, HtmlBody, TextBody);

    /// <summary>Error text as stored: single line, truncated.</summary>
    public static string TruncateError(string error)
    {
        var singleLine = error.ReplaceLineEndings(" ");
        return singleLine.Length > LastErrorMaxLength ? singleLine[..LastErrorMaxLength] : singleLine;
    }
}
