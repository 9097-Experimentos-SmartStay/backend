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
///     The message is enlisted in the current unit of work (transactional outbox): call it BEFORE the
///     <c>IUnitOfWork</c> commits the change the e-mail announces (domain event handlers already run inside that
///     transaction). The e-mail is then stored if and only if the change commits (never for a rolled-back change)
///     and is delivered in the background afterwards, surviving restarts, with retries. The call never waits for
///     the mail server, so the response time of a request does not reveal whether an e-mail was sent (US-04: no
///     account enumeration).
/// </remarks>
public interface IEmailSender
{
    /// <summary>Accepts <paramref name="message"/> for delivery once the current unit of work commits.</summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
