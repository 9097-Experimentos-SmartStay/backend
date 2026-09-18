using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration;

/// <summary>
///     Marks every operation that is not <see cref="IAllowAnonymous"/> as requiring the bearer scheme and
///     documents its 401/403 ProblemDetails responses.
/// </summary>
public class BearerSecurityRequirementOperationFilter : IOperationFilter
{
    public const string SchemeId = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any()) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = SchemeId, Type = ReferenceType.SecurityScheme }
            }] = Array.Empty<string>()
        });

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Missing, invalid, expired or revoked token (ProblemDetails)." });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "The authenticated user is not allowed (ProblemDetails)." });
    }
}
