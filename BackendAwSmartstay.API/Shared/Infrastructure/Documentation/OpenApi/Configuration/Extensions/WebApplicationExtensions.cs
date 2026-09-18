using Scalar.AspNetCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring the API documentation and CORS middleware in the application pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    ///     Public API documentation in every environment (US-32):
    ///     <list type="bullet">
    ///         <item><c>/scalar</c>: interactive API reference (operations grouped by tag, request/response schemas
    ///         and examples, "try it" with the bearer token, code samples in several languages);</item>
    ///         <item><c>/swagger</c>: Swagger UI; <c>/swagger/v1/swagger.json</c>: the OpenAPI document;</item>
    ///         <item><c>/</c> redirects to <c>/scalar</c>.</item>
    ///     </list>
    /// </summary>
    public static void MapApiDocumentation(this WebApplication app)
    {
        app.MapSwagger().AllowAnonymous();

        app.MapScalarApiReference("/scalar", options => options
                .WithTitle("SmartStay API reference")
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .AddPreferredSecuritySchemes(BearerSecurityRequirementOperationFilter.SchemeId)
                .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
                .DisableTelemetry()
                .DisableAgent())
            .AllowAnonymous();

        app.MapGet("/", () => Results.Redirect("/scalar")).AllowAnonymous().ExcludeFromDescription();
    }

    /// <summary>Swagger UI at <c>/swagger</c> (static middleware before authentication, so it stays public).</summary>
    public static void UseOpenApiConfiguration(this WebApplication app)
    {
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartStay API v1");
            c.RoutePrefix = "swagger";
        });
    }

    /// <summary>
    /// Applies the configured CORS policy (origins come from <c>Cors:AllowedOrigins</c>).
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static void UseCorsPolicy(this WebApplication app)
    {
        var origins = WebApplicationBuilderExtensions.GetAllowedOrigins(app.Configuration);
        if (origins.Length == 0)
            app.Logger.LogWarning("CORS: no origins configured (Cors__AllowedOrigins). Cross-origin browser requests will be rejected.");
        else
            app.Logger.LogInformation("CORS: allowed origins: {Origins}", string.Join(", ", origins));

        app.UseCors(WebApplicationBuilderExtensions.CorsPolicyName);
    }
}
