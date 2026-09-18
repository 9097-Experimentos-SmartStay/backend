using BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration;

/// <summary>
///     Documents how each operation is authenticated: anonymous operations get nothing, scheduler operations
///     (<see cref="ScheduledJobsAuthenticationExtensions.RunScheduledJobsPolicy"/>) the <c>X-Cron-Key</c> scheme, and
///     every other operation the bearer scheme plus its 401/403 ProblemDetails responses.
/// </summary>
public class BearerSecurityRequirementOperationFilter : IOperationFilter
{
    public const string SchemeId = "Bearer";
    public const string CronKeySchemeId = "CronKey";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var policies = metadata.OfType<IAuthorizeData>().Select(data => data.Policy).ToList();
        var isScheduledJob = policies.Contains(ScheduledJobsAuthenticationExtensions.RunScheduledJobsPolicy);
        if (!isScheduledJob && metadata.OfType<IAllowAnonymous>().Any()) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = isScheduledJob ? CronKeySchemeId : SchemeId, Type = ReferenceType.SecurityScheme }
            }] = Array.Empty<string>()
        });

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Missing, invalid, expired or revoked credentials (ProblemDetails)." });
        if (!isScheduledJob)
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "The authenticated user is not allowed (ProblemDetails)." });
    }
}
