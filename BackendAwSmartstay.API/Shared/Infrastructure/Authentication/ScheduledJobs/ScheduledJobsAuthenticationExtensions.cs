using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;

public static class ScheduledJobsAuthenticationExtensions
{
    /// <summary>Policy of the endpoints the external scheduler calls (authenticated by <c>X-Cron-Key</c>, never by a user token).</summary>
    public const string RunScheduledJobsPolicy = "RunScheduledJobs";

    /// <summary>Registers the <c>X-Cron-Key</c> scheme, its validated settings and the <see cref="RunScheduledJobsPolicy"/>.</summary>
    public static IServiceCollection AddScheduledJobsAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CronSettings>()
            .Bind(configuration.GetSection(CronSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, CronApiKeyAuthenticationHandler>(CronApiKeyAuthenticationHandler.SchemeName, null);

        services.AddAuthorizationBuilder()
            .AddPolicy(RunScheduledJobsPolicy, policy => policy
                .AddAuthenticationSchemes(CronApiKeyAuthenticationHandler.SchemeName)
                .RequireClaim(CronApiKeyAuthenticationHandler.ScopeClaim, CronApiKeyAuthenticationHandler.ScheduledJobsScope));

        return services;
    }
}
