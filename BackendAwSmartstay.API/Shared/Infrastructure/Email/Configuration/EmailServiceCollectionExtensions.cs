using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Delivery;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;

public static class EmailServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the <see cref="IEmailSender"/> port (queued, delivered in the background) and the transport
    ///     chosen from <see cref="EmailSettings"/>: SMTP when configured, otherwise the log (never in Production,
    ///     where the options validation stops the application at startup). Also binds <see cref="ApplicationUrlsSettings"/>
    ///     used to build the links of the e-mails.
    /// </summary>
    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApplicationUrlsSettings>()
            .Bind(configuration.GetSection(ApplicationUrlsSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>();

        services.AddSingleton<EmailDeliveryQueue>();
        services.AddSingleton<IEmailSender, QueuedEmailSender>();
        services.AddSingleton<SmtpEmailTransport>();
        services.AddSingleton<LoggingEmailTransport>();
        services.AddSingleton<IEmailTransport>(provider =>
            provider.GetRequiredService<IOptions<EmailSettings>>().Value.IsSmtpConfigured
                ? provider.GetRequiredService<SmtpEmailTransport>()
                : provider.GetRequiredService<LoggingEmailTransport>());
        services.AddHostedService<EmailDeliveryWorker>();

        return services;
    }
}
