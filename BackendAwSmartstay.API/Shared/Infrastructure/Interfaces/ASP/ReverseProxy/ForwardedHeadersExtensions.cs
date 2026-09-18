using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using ProxyNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ReverseProxy;

public static class ForwardedHeadersExtensions
{
    /// <summary>
    ///     Resolves the real client address behind Cloudflare and Render's load balancer with the native
    ///     <see cref="ForwardedHeadersOptions"/>: <c>X-Forwarded-For</c> is walked from right to left over the whole
    ///     chain (<c>ForwardLimit = null</c>), skipping only hops inside <see cref="ForwardedHeadersSettings"/>'s trusted
    ///     networks. The first untrusted address becomes <c>HttpContext.Connection.RemoteIpAddress</c>, so entries a
    ///     client writes to the left of it are ignored. Replaces the <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED</c>
    ///     shortcut, which trusts every proxy but unwraps a single hop (it returned Render's internal address).
    /// </summary>
    public static IServiceCollection AddSmartStayForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ForwardedHeadersSettings>()
            .Bind(configuration.GetSection(ForwardedHeadersSettings.SectionName))
            .Validate(settings => settings.EffectiveTrustedNetworks.All(cidr => ProxyNetwork.TryParse(cidr, out _)),
                $"{ForwardedHeadersSettings.SectionName}:TrustedNetworks must contain valid CIDR networks (e.g. 10.0.0.0/8).")
            .ValidateOnStart();

        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<ForwardedHeadersSettings>>((options, settings) =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = null;
                options.KnownProxies.Clear();
                options.KnownNetworks.Clear();
                foreach (var cidr in settings.Value.EffectiveTrustedNetworks)
                    options.KnownNetworks.Add(ProxyNetwork.Parse(cidr));
            });

        return services;
    }

    /// <summary>Must be the first middleware: everything after it (rate limiting, audit) reads the client address.</summary>
    public static WebApplication UseSmartStayForwardedHeaders(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ForwardedHeadersExtensions));
        var options = app.Services.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
        logger.LogInformation("Forwarded headers: trusting {TrustedNetworkCount} proxy networks", options.KnownNetworks.Count);
        if (app.Configuration.GetValue<bool>("ASPNETCORE_FORWARDEDHEADERS_ENABLED") ||
            app.Configuration.GetValue<bool>("FORWARDEDHEADERS_ENABLED"))
            logger.LogWarning("ASPNETCORE_FORWARDEDHEADERS_ENABLED is set but no longer needed; remove it " +
                              "(the forwarded headers are configured in code)");

        app.UseForwardedHeaders();
        return app;
    }
}
