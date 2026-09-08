using System.Net.Mime;
using BackendAwSmartstay.API.Analytics.Domain.Model.Queries;
using BackendAwSmartstay.API.Analytics.Domain.Services;
using BackendAwSmartstay.API.Analytics.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Analytics.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using BackendAwSmartstay.API.Shared.Infrastructure.Resilience;
using Polly.CircuitBreaker;
using BackendAwSmartstay.API.Shared.Infrastructure.Messaging;

namespace BackendAwSmartstay.API.Analytics.Interfaces.REST;

//REST controller for analytics operations.
//[Authorize(UserRoles.Admin, UserRoles.ChainAdmin)] // Only Admin/ChainAdmin should access this
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Analytics Endpoints")]
/*public class AnalyticsController(IAnalyticsQueryService analyticsQueryService, IConnectionMultiplexer redis)
    : ControllerBase*/
public class AnalyticsController(
    IAnalyticsQueryService analyticsQueryService,
    IConnectionMultiplexer redis,
    ActiveMqProducer activeMqProducer)
    : ControllerBase
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly ActiveMqProducer _producer = activeMqProducer;

    //  Retrieves monthly performance metrics.
    // <returns>An action result containing the performance metrics resource.</returns>
    [HttpGet("performance/monthly")]
    [SwaggerOperation(
        Summary = "Get monthly performance metrics",
        Description = "Retrieves aggregated metrics like revenue and occupancy for the current month.",
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
/*
    [HttpPost("cache")]
    public async Task<IActionResult> CacheData([FromBody] string message)
    {
        try
        {
            await RedisCircuitBreaker.CircuitBreaker.ExecuteAsync(async () =>
            {
                await _db.ListRightPushAsync("analytics-messages", message);
            });

            return Ok(new
            {
                success = true,
                saved = message
            });
        }
        catch (BrokenCircuitException)
        {
            return StatusCode(503, new
            {
                success = false,
                message = "Circuit Breaker is OPEN."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }
*/
    [HttpPost("cache")]
    public async Task<IActionResult> CacheData([FromBody] string message)
    {
        try
        {
            await RedisCircuitBreaker.CircuitBreaker.ExecuteAsync(async () =>
            {
                await _db.ListRightPushAsync(
                    "analytics-messages",
                    message
                );
            });

            return Ok(new
            {
                success = true,
                source = "Redis",
                saved = message
            });
        }
        catch (BrokenCircuitException)
        {
            _producer.Send(
                "analytics-fallback",
                message
            );

            return Ok(new
            {
                success = true,
                source = "ActiveMQ",
                saved = message
            });
        }
        catch (Exception)
        {
            _producer.Send(
                "analytics-fallback",
                message
            );

            return Ok(new
            {
                success = true,
                source = "ActiveMQ",
                saved = message
            });
        }
    }
    
    [HttpGet("cache")]
    public async Task<IActionResult> GetCache()
    {
        try
        {
            var values = await RedisCircuitBreaker.CircuitBreaker.ExecuteAsync(async () =>
            {
                return await _db.ListRangeAsync("analytics-messages");
            });

            return Ok(values.Select(v => v.ToString()));
        }
        catch (BrokenCircuitException)
        {
            return StatusCode(503, new
            {
                success = false,
                message = "Circuit Breaker is OPEN."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}