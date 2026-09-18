using System.Net;
using System.Net.Http.Headers;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Outbox;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;

public static class EmailServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the <see cref="IEmailSender"/> port (transactional outbox), the <see cref="IEmailDispatcher"/>
    ///     with its background worker, and the transport chosen by <c>Email:Transport</c> (<see cref="EmailSettings"/>,
    ///     validated at startup). Also binds <see cref="ApplicationUrlsSettings"/> used to build the links of the e-mails.
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
        services.TryAddSingleton(TimeProvider.System);

        // Outbox: the e-mail is stored with the business change (same unit of work) and delivered afterwards.
        services.AddScoped<IEmailSender, OutboxEmailSender>();
        services.AddScoped<IEmailDispatcher, OutboxEmailDispatcher>();
        services.AddHostedService<OutboxEmailDispatchWorker>();

        services.AddSingleton<LoggingEmailTransport>();
        services.AddSingleton<SmtpEmailTransport>();
        services.AddBrevoApiTransport();
        services.AddScoped<IEmailTransport>(provider =>
            provider.GetRequiredService<IOptions<EmailSettings>>().Value.EffectiveTransport switch
            {
                EmailTransportKind.BrevoApi => provider.GetRequiredService<BrevoApiEmailTransport>(),
                EmailTransportKind.Smtp => provider.GetRequiredService<SmtpEmailTransport>(),
                _ => provider.GetRequiredService<LoggingEmailTransport>()
            });

        return services;
    }

    /// <summary>
    ///     Typed <see cref="HttpClient"/> of the Brevo API (<c>IHttpClientFactory</c>: pooled handlers, DNS refresh)
    ///     with a resilience pipeline of Microsoft.Extensions.Http.Resilience (Polly v8, already a dependency):
    ///     <list type="bullet">
    ///         <item>total timeout, then per-attempt timeout: a hung connection never blocks the outbox;</item>
    ///         <item>up to 2 quick retries (exponential, jitter, honours <c>Retry-After</c>) only where the request
    ///         certainly did not create an e-mail: connection/DNS/TLS failures, 429 and 502/503. A timeout or a 500 is
    ///         ambiguous (Brevo may have accepted it), so it is not retried here: the outbox retries it later with
    ///         backoff, which is the durable retry policy anyway.</item>
    ///     </list>
    ///     The standard handler was not used as is because it retries every transient outcome of a POST, which could
    ///     send duplicates, and its circuit breaker never trips at this volume.
    /// </summary>
    private static void AddBrevoApiTransport(this IServiceCollection services)
    {
        services.AddHttpClient<BrevoApiEmailTransport>((provider, client) =>
            {
                var brevo = provider.GetRequiredService<IOptions<EmailSettings>>().Value.Brevo;
                client.BaseAddress = new Uri(brevo.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = Timeout.InfiniteTimeSpan; // the resilience pipeline owns the timeouts
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(brevo.ApiKey))
                    client.DefaultRequestHeaders.Add(BrevoApiEmailTransport.ApiKeyHeader, brevo.ApiKey);
            })
            .AddResilienceHandler("brevo-api", (pipeline, context) =>
            {
                var brevo = context.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>().Value.Brevo;
                pipeline
                    .AddTimeout(TimeSpan.FromSeconds(brevo.TotalTimeoutSeconds))
                    .AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.FromSeconds(1),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        ShouldRetryAfterHeader = true,
                        ShouldHandle = args => ValueTask.FromResult(args.Outcome switch
                        {
                            { Exception: HttpRequestException
                            {
                                HttpRequestError: HttpRequestError.NameResolutionError
                                or HttpRequestError.ConnectionError
                                or HttpRequestError.SecureConnectionError
                            } } => true,
                            { Result.StatusCode: HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable } => true,
                            _ => false
                        })
                    })
                    .AddTimeout(TimeSpan.FromSeconds(brevo.AttemptTimeoutSeconds));
            });
    }
}
