namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;

/// <summary>
///     E-mail delivery settings (section <c>Email</c>, env vars <c>Email__Smtp__*</c> and <c>Email__From__*</c>).
///     Production uses an SMTP relay (Brevo). Without SMTP settings, non-production environments write the
///     e-mails to the log instead; Production refuses to start (see <see cref="EmailSettingsValidator"/>).
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    public SmtpSettings Smtp { get; set; } = new();

    public SenderSettings From { get; set; } = new();

    /// <summary>True when an SMTP relay is configured.</summary>
    public bool IsSmtpConfigured => !string.IsNullOrWhiteSpace(Smtp.Host);

    /// <summary>SMTP relay, e.g. Brevo: <c>smtp-relay.brevo.com</c>, port 587, STARTTLS.</summary>
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
}
