namespace BackendAwSmartstay.API.Shared.Application.OutboundServices;

/// <summary>Outcome of a delivery round of the pending e-mails.</summary>
/// <param name="Sent">E-mails accepted by the mail system.</param>
/// <param name="Retrying">E-mails that failed transiently and were scheduled for another attempt.</param>
/// <param name="Failed">E-mails given up (permanent rejection or last attempt).</param>
public sealed record EmailDispatchReport(int Sent, int Retrying, int Failed)
{
    public static readonly EmailDispatchReport Empty = new(0, 0, 0);

    public static EmailDispatchReport operator +(EmailDispatchReport left, EmailDispatchReport right) =>
        new(left.Sent + right.Sent, left.Retrying + right.Retrying, left.Failed + right.Failed);
}

/// <summary>
///     Delivers the e-mails accepted by <see cref="IEmailSender"/> that are due. Runs in the background and from
///     the scheduled job <c>POST /api/v1/emails/dispatch</c> (the host may sleep between requests). Idempotent and
///     safe to run concurrently on several instances: an e-mail is claimed by one of them only.
/// </summary>
public interface IEmailDispatcher
{
    Task<EmailDispatchReport> DispatchDueAsync(CancellationToken cancellationToken = default);
}
