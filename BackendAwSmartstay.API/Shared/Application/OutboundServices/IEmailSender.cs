namespace BackendAwSmartstay.API.Shared.Application.OutboundServices;

/// <summary>
///     An e-mail ready to be delivered: a subject plus an HTML body and its plain-text alternative.
/// </summary>
/// <param name="To">Recipient address.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="HtmlBody">HTML body.</param>
/// <param name="TextBody">Plain-text alternative of <paramref name="HtmlBody"/>.</param>
public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);

/// <summary>
///     Outbound port used by the application layer of every bounded context to send e-mails.
/// </summary>
/// <remarks>
///     Call it only after the unit of work has committed: an e-mail must never announce a change that was rolled
///     back. The adapter accepts the message for delivery and returns without waiting for the mail server, so the
///     response time of a request does not reveal whether an e-mail was sent (US-04: no account enumeration).
/// </remarks>
public interface IEmailSender
{
    /// <summary>Accepts <paramref name="message"/> for delivery.</summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
