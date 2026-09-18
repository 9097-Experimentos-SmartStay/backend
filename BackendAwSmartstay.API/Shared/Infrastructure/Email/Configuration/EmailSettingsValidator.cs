using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;

/// <summary>
///     Validates <see cref="EmailSettings"/> when the host starts (options pattern + <c>ValidateOnStart</c>):
///     <list type="bullet">
///         <item>Production must use a real transport (<c>BrevoApi</c> with its API key, or <c>Smtp</c> with its
///         host): with the log transport no e-mail (verification, password reset...) would ever leave the server,
///         so the application refuses to start;</item>
///         <item>the chosen transport, the sender and the outbox settings must be consistent.</item>
///     </list>
/// </summary>
public class EmailSettingsValidator(IHostEnvironment environment) : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings settings)
    {
        var failures = new List<string>();
        var transport = settings.EffectiveTransport;

        if (environment.IsProduction() && settings.Transport is null or EmailTransportKind.Log)
            failures.Add("Email:Transport must be 'BrevoApi' or 'Smtp' in Production. Set 'Email__Transport=BrevoApi' " +
                         "and 'Email__Brevo__ApiKey' (or 'Email__Transport=Smtp' and 'Email__Smtp__*').");

        switch (transport)
        {
            case EmailTransportKind.BrevoApi:
                ValidateBrevo(settings.Brevo, failures);
                break;
            case EmailTransportKind.Smtp:
                ValidateSmtp(settings.Smtp, failures);
                break;
        }

        if (transport != EmailTransportKind.Log || environment.IsProduction())
        {
            if (string.IsNullOrWhiteSpace(settings.From.Address) || !MailAddress.TryCreate(settings.From.Address, out _))
                failures.Add("Email:From:Address must be a valid e-mail address (the verified sender of the mail provider).");
        }

        ValidateOutbox(settings, failures);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateBrevo(EmailSettings.BrevoSettings brevo, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(brevo.ApiKey))
            failures.Add("Email:Brevo:ApiKey is not configured. Set 'Email__Brevo__ApiKey' (Brevo > SMTP & API > API keys).");
        if (!Uri.TryCreate(brevo.BaseUrl, UriKind.Absolute, out var baseUrl) || baseUrl.Scheme is not ("https" or "http"))
            failures.Add("Email:Brevo:BaseUrl must be an absolute http(s) URL (default https://api.brevo.com).");
        if (brevo.AttemptTimeoutSeconds is < 1 or > 120)
            failures.Add("Email:Brevo:AttemptTimeoutSeconds must be between 1 and 120.");
        if (brevo.TotalTimeoutSeconds < brevo.AttemptTimeoutSeconds || brevo.TotalTimeoutSeconds > 300)
            failures.Add("Email:Brevo:TotalTimeoutSeconds must be between AttemptTimeoutSeconds and 300.");
    }

    private static void ValidateSmtp(EmailSettings.SmtpSettings smtp, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(smtp.Host))
            failures.Add("Email:Smtp:Host is not configured. Set 'Email__Smtp__Host' (and Port, Username, Password).");
        if (smtp.Port is < 1 or > 65535)
            failures.Add("Email:Smtp:Port must be between 1 and 65535.");
        if (string.IsNullOrWhiteSpace(smtp.Username) != string.IsNullOrWhiteSpace(smtp.Password))
            failures.Add("Email:Smtp:Username and Email:Smtp:Password must be set together.");
        if (smtp.TimeoutSeconds is < 1 or > 300)
            failures.Add("Email:Smtp:TimeoutSeconds must be between 1 and 300.");
    }

    private static void ValidateOutbox(EmailSettings settings, List<string> failures)
    {
        var outbox = settings.Outbox;
        if (outbox.PollIntervalSeconds is < 1 or > 3600)
            failures.Add("Email:Outbox:PollIntervalSeconds must be between 1 and 3600.");
        if (outbox.BatchSize is < 1 or > 500)
            failures.Add("Email:Outbox:BatchSize must be between 1 and 500.");
        if (outbox.MaxAttempts is < 1 or > 50)
            failures.Add("Email:Outbox:MaxAttempts must be between 1 and 50.");
        if (outbox.InitialRetryDelaySeconds < 1)
            failures.Add("Email:Outbox:InitialRetryDelaySeconds must be at least 1.");
        if (outbox.MaxRetryDelayMinutes < 1)
            failures.Add("Email:Outbox:MaxRetryDelayMinutes must be at least 1.");
        // A claimed e-mail must not become due again while it is still being sent.
        var longestDelivery = settings.EffectiveTransport switch
        {
            EmailTransportKind.BrevoApi => settings.Brevo.TotalTimeoutSeconds,
            EmailTransportKind.Smtp => settings.Smtp.TimeoutSeconds * 4, // connect, authenticate, send, quit
            _ => 0
        };
        if (outbox.LeaseSeconds <= longestDelivery)
            failures.Add($"Email:Outbox:LeaseSeconds must exceed the longest delivery of the transport ({longestDelivery} s).");
    }
}
