using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;

/// <summary>
///     Validates <see cref="EmailSettings"/> when the host starts (options pattern + <c>ValidateOnStart</c>):
///     <list type="bullet">
///         <item>Production must have an SMTP relay: without it no e-mail (verification, password reset...) would
///         ever leave the server, so the application refuses to start;</item>
///         <item>when SMTP is configured, the port, the sender and the credentials must be consistent.</item>
///     </list>
/// </summary>
public class EmailSettingsValidator(IHostEnvironment environment) : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings settings)
    {
        var failures = new List<string>();

        if (!settings.IsSmtpConfigured)
        {
            if (environment.IsProduction())
                failures.Add("Email:Smtp:Host is not configured. Production needs an SMTP relay: set 'Email__Smtp__Host', " +
                             "'Email__Smtp__Port', 'Email__Smtp__Username', 'Email__Smtp__Password' and 'Email__From__Address'.");
        }
        else
        {
            if (settings.Smtp.Port is < 1 or > 65535)
                failures.Add("Email:Smtp:Port must be between 1 and 65535.");
            if (string.IsNullOrWhiteSpace(settings.Smtp.Username) != string.IsNullOrWhiteSpace(settings.Smtp.Password))
                failures.Add("Email:Smtp:Username and Email:Smtp:Password must be set together.");
            if (settings.Smtp.TimeoutSeconds is < 1 or > 300)
                failures.Add("Email:Smtp:TimeoutSeconds must be between 1 and 300.");
        }

        if (settings.IsSmtpConfigured || environment.IsProduction())
        {
            if (string.IsNullOrWhiteSpace(settings.From.Address) || !MailAddress.TryCreate(settings.From.Address, out _))
                failures.Add("Email:From:Address must be a valid e-mail address (the verified sender of the SMTP relay).");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
