using System.Globalization;
using System.Threading.RateLimiting;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;

public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>
    ///     Native ASP.NET Core rate limiting: fixed windows partitioned by client IP, one policy per kind of
    ///     anonymous endpoint (<see cref="RateLimitPolicies"/>). Rejections are 429 ProblemDetails with
    ///     <c>Retry-After</c>.
    /// </summary>
    public static IServiceCollection AddSmartStayRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingSettings>()
            .Bind(configuration.GetSection(RateLimitingSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitingSettings>>((options, settings) =>
            {
                var limits = settings.Value;
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(RateLimitPolicies.Credentials, context =>
                    PerClientIp(context, limits.CredentialsPermitLimit, limits.CredentialsWindowSeconds));
                options.AddPolicy(RateLimitPolicies.PublicForms, context =>
                    PerClientIp(context, limits.PublicFormsPermitLimit, limits.PublicFormsWindowSeconds));
                options.OnRejected = WriteProblemAsync;
            });

        return services;
    }

    private static RateLimitPartition<string> PerClientIp(HttpContext context, int permitLimit, int windowSeconds) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0
            });

    private static async ValueTask WriteProblemAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            httpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Too many attempts from this network. Wait a moment and try again.",
                Extensions = { [ProblemCodes.CodeExtension] = ProblemCodes.RateLimited }
            }
        });
    }
}
