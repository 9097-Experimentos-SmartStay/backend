namespace BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI and CORS middleware in the application pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures OpenAPI/Swagger middleware for API documentation and testing interface.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <remarks>
    /// Enables Swagger UI at /swagger endpoint for interactive API documentation.
    /// </remarks>
    public static void UseOpenApiConfiguration(this WebApplication app)
    {
        // The OpenAPI document is an endpoint (MapSwagger().AllowAnonymous() in Program.cs); the UI is static
        // middleware that runs before authentication, so the interactive docs stay public (US-32).
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackendAwSmartstay API v1");
            
            // Sugerencia: Deja la ruta vacía para ingresar a Swagger desde el link principal de Render
            c.RoutePrefix = string.Empty; 
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
