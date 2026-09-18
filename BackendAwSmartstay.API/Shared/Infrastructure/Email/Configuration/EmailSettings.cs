namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;

/// <summary>
///     E-mail delivery settings (section <c>Email</c>, env vars <c>Email__*</c>).
/// </summary>
/// <remarks>
///     The transport is chosen explicitly with <c>Email:Transport</c>: <see cref="EmailTransportKind.BrevoApi"/>
///     (Brevo transactional HTTP API, used in production), <see cref="EmailTransportKind.Smtp"/> (any SMTP relay,
///     e.g. a local test server) or <see cref="EmailTransportKind.Log"/> (the e-mail is written to the log; never
///     allowed in Production). When it is not set, environments other than Production use the log; Production
///     refuses to start (see <see cref="EmailSettingsValidator"/>).
/// </remarks>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>Transport that hands the e-mails to a mail system. <c>null</c> = not chosen.</summary>
    public EmailTransportKind? Transport { get; set; }

    public BrevoSettings Brevo { get; set; } = new();

    public SmtpSettings Smtp { get; set; } = new();

    public SenderSettings From { get; set; } = new();

    public OutboxSettings Outbox { get; set; } = new();

    /// <summary>The transport in use: the configured one, or the log when none was chosen (outside Production).</summary>
    public EmailTransportKind EffectiveTransport => Transport ?? EmailTransportKind.Log;

    /// <summary>Brevo transactional e-mail API (<c>POST /v3/smtp/email</c>), over HTTPS (port 443).</summary>
    public class BrevoSettings
    {
        /// <summary>API key of Brevo (Settings > SMTP &amp; API > API keys). Secret: never logged.</summary>
        public string? ApiKey { get; set; }

        public string BaseUrl { get; set; } = "https://api.brevo.com";

        /// <summary>Timeout of one HTTP attempt.</summary>
        public int AttemptTimeoutSeconds { get; set; } = 10;

        /// <summary>Timeout of one delivery, retries of the connection failures included.</summary>
        public int TotalTimeoutSeconds { get; set; } = 30;
    }

    /// <summary>SMTP relay, e.g. a local test server, or Brevo: <c>smtp-relay.brevo.com</c>, port 587, STARTTLS.</summary>
    public class SmtpSettings
    {
        public string? Host { get; set; }

        /// <summary>587 (STARTTLS) by default; 465 means implicit TLS.</summary>
        public int Port { get; set; } = 587;

        public string? Username { get; set; }

        public string? Password { get; set; }

        /// <summary>Require TLS (STARTTLS on 587, implicit TLS on 465). Only disable it for a local test server.</summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>Connection/command timeout.</summary>
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>Sender shown to recipients. Brevo requires a verified sender address.</summary>
    public class SenderSettings
    {
        public string? Address { get; set; }

        public string Name { get; set; } = "SmartStay";
    }

    /// <summary>
    ///     Transactional outbox: every e-mail is stored in <c>outbox_emails</c> with the change that caused it and
    ///     delivered by a background dispatcher (and by the <c>POST /api/v1/emails/dispatch</c> scheduled job).
    /// </summary>
    public class OutboxSettings
    {
        /// <summary>How often the background dispatcher looks for due e-mails.</summary>
        public int PollIntervalSeconds { get; set; } = 10;

        /// <summary>E-mails claimed per round.</summary>
        public int BatchSize { get; set; } = 20;

        /// <summary>Delivery attempts before an e-mail is given up (status <c>Failed</c>).</summary>
        public int MaxAttempts { get; set; } = 8;

        /// <summary>Delay before the first retry; it doubles on every failed attempt (with jitter).</summary>
        public int InitialRetryDelaySeconds { get; set; } = 30;

        /// <summary>Upper bound of the retry delay.</summary>
        public int MaxRetryDelayMinutes { get; set; } = 60;

        /// <summary>
        ///     How long a claimed e-mail stays reserved for the instance that claimed it. If that instance dies
        ///     mid-delivery, the e-mail is due again once the lease expires. Must exceed the transport timeout.
        /// </summary>
        public int LeaseSeconds { get; set; } = 120;
    }
}

/// <summary>Mail system the e-mails are handed to.</summary>
public enum EmailTransportKind
{
    /// <summary>Written to the log (development only).</summary>
    Log,

    /// <summary>SMTP relay (MailKit).</summary>
    Smtp,

    /// <summary>Brevo transactional e-mail HTTP API.</summary>
    BrevoApi
}
