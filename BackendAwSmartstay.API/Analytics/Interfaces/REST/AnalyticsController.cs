using System.Net.Mime;
using BackendAwSmartstay.API.Analytics.Domain.Model.Queries;
using BackendAwSmartstay.API.Analytics.Domain.Services;
using BackendAwSmartstay.API.Analytics.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Analytics.Interfaces.REST.Transform;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using BackendAwSmartstay.API.Shared.Infrastructure.Resilience;
using Polly.CircuitBreaker;
using BackendAwSmartstay.API.Shared.Infrastructure.Messaging;

namespace BackendAwSmartstay.API.Analytics.Interfaces.REST;

/// <summary>
///     REST controller for analytics operations.
/// </summary>
/// <remarks>
///     The <c>/cache</c> endpoints are an optional resilience lab (Redis + Polly circuit breaker + ActiveMQ fallback).
///     Redis and ActiveMQ are only registered when <c>ConnectionStrings:RedisConnection</c> and
///     <c>Messaging:ActiveMqBrokerUri</c> are configured; otherwise those endpoints answer 503.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Analytics Endpoints")]
public class AnalyticsController(
    IAnalyticsQueryService analyticsQueryService,
    IConnectionMultiplexer? redis = null,
    ActiveMqProducer? activeMqProducer = null)
    : ControllerBase
{
    private const string CacheListKey = "analytics-messages";
    private const string FallbackQueue = "analytics-fallback";
    private const int MaxMessageLength = 1024;
    private const int MaxCachedMessages = 1000;

    //  Retrieves monthly performance metrics.
    // <returns>An action result containing the performance metrics resource.</returns>
    [HttpGet("performance/monthly")]
    [Authorize(Policy = Policies.ViewAnalytics)]
    [SwaggerOperation(
        Summary = "Get monthly performance metrics",
        Description = "Retrieves aggregated metrics like revenue and occupancy for the current month. Requires Admin or ChainAdmin.",
        OperationId = "GetMonthlyPerformance")]
    [SwaggerResponse(StatusCodes.Status200OK, "The metrics", typeof(PerformanceMetricsResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid JWT Token")]
    [SwaggerResponse(StatusCodes.Status403Forbidden,
        "User does not have required permissions (Requires Admin/ChainAdmin)")]
    public async Task<IActionResult> GetMonthlyPerformance()
    {
        var query = new GetMonthlyPerformanceQuery();
        var metrics = await analyticsQueryService.Handle(query);
        var resource = PerformanceMetricsAssembler.ToResourceFromEntity(metrics);
        return Ok(resource);
    }

    [HttpPost("cache")]
    [Authorize(Policy = Policies.OperateAnalyticsLab)]
    [SwaggerOperation(
        Summary = "Push a message to the analytics cache (resilience lab)",
        Description = "Writes to Redis through a circuit breaker; falls back to ActiveMQ. 503 when the lab is not configured. ChainAdmin only.",
        OperationId = "CacheAnalyticsMessage")]
    public async Task<IActionResult> CacheData([FromBody] string message)
    {
        if (redis is null) return CacheDisabled();

        if (string.IsNullOrWhiteSpace(message) || message.Length > MaxMessageLength)
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid message",
                detail: $"The message must be a non-empty string of at most {MaxMessageLength} characters.");

        try
        {
            await RedisCircuitBreaker.CircuitBreaker.ExecuteAsync(async () =>
            {
                var db = redis.GetDatabase();
                await db.ListRightPushAsync(CacheListKey, message);
                // Keep the list bounded: only the most recent messages are retained.
                await db.ListTrimAsync(CacheListKey, -MaxCachedMessages, -1);
            });

            return Ok(new
            {
                success = true,
                source = "Redis",
                saved = message
            });
        }
        catch (Exception)
        {
            // Redis failed or the circuit is open: fall back to ActiveMQ if configured.
            if (activeMqProducer is null)
                return Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Analytics cache unavailable",
                    detail: "Redis is unavailable and no ActiveMQ fallback is configured (Messaging__ActiveMqBrokerUri).");

            try
            {
                activeMqProducer.Send(FallbackQueue, message);
            }
            catch (Exception)
            {
                return Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Analytics cache unavailable",
                    detail: "Redis and the ActiveMQ fallback are both unavailable.");
            }

            return Ok(new
            {
                success = true,
                source = "ActiveMQ",
                saved = message
            });
        }
    }
    
    [HttpGet("cache")]
    [Authorize(Policy = Policies.OperateAnalyticsLab)]
    [SwaggerOperation(
        Summary = "Read the analytics cache (resilience lab)",
        Description = "Reads the cached messages from Redis through a circuit breaker. 503 when the lab is not configured or the circuit is open. ChainAdmin only.",
        OperationId = "GetAnalyticsCache")]
    public async Task<IActionResult> GetCache()
    {
        if (redis is null) return CacheDisabled();

        try
        {
            var values = await RedisCircuitBreaker.CircuitBreaker.ExecuteAsync(async () =>
            {
                return await redis.GetDatabase().ListRangeAsync(CacheListKey);
            });

            return Ok(values.Select(v => v.ToString()));
        }
        catch (BrokenCircuitException)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Circuit Breaker is OPEN.",
                detail: "Redis failed repeatedly; retry later.");
        }
        catch (Exception)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Analytics cache unavailable",
                detail: "Redis is not reachable.");
        }
    }

    private ObjectResult CacheDisabled() => Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "Analytics cache disabled",
        detail: "The analytics cache is not configured on this deployment. Set ConnectionStrings__RedisConnection " +
                "(and optionally Messaging__ActiveMqBrokerUri) to enable it.");
}
