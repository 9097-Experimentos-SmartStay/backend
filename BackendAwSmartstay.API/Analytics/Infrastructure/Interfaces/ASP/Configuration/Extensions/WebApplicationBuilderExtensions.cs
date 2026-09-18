using BackendAwSmartstay.API.Analytics.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Analytics.Domain.Repositories;
using BackendAwSmartstay.API.Analytics.Domain.Services;
using BackendAwSmartstay.API.Analytics.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Messaging;
using StackExchange.Redis;

namespace BackendAwSmartstay.API.Analytics.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddAnalyticsContextServices(this WebApplicationBuilder builder)
    {
        // Analytics Bounded Context Injection Configuration

        // Repositories
        builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        // Query Services
        builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        
        // Note: Commands are not implemented yet as Analytics is currently Read-Only
    }

    /// <summary>
    ///     Registers the optional analytics cache lab infrastructure (Redis + ActiveMQ fallback).
    ///     Nothing is registered (and nothing connects) unless the corresponding settings are present:
    ///     <c>ConnectionStrings:RedisConnection</c> and <c>Messaging:ActiveMqBrokerUri</c>.
    ///     Without Redis the <c>/api/v1/analytics/cache</c> endpoints answer 503.
    /// </summary>
    public static void AddAnalyticsCacheServices(this WebApplicationBuilder builder)
    {
        var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            redisOptions.AbortOnConnectFail = false; // the app must boot even if Redis is down
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        }

        var brokerUri = builder.Configuration["Messaging:ActiveMqBrokerUri"];
        if (!string.IsNullOrWhiteSpace(brokerUri))
        {
            builder.Services.AddSingleton(serviceProvider => new ActiveMqProducer(
                brokerUri, serviceProvider.GetRequiredService<ILogger<ActiveMqProducer>>()));
        }
    }
}
