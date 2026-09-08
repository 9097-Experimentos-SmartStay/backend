using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Domain.Repositories;
using BackendAwSmartstay.API.Profiles.Domain.Services;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
/// Provides extension methods for configuring Profile context services in the application.
/// </summary>
/// <remarks>
/// This static class extends <see cref="WebApplicationBuilder"/> to enable fluent configuration
/// of dependency injection for the Profiles bounded context.
/// </remarks>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Registers all Profile-related services into the dependency injection container.
    /// </summary>
    /// <param name="builder">The web application builder instance to extend.</param>
    /// <remarks>
    /// This method configures the following services with scoped lifetime:
    /// <list type="bullet">
    /// <item><description><see cref="IProfileRepository"/> - Data access layer for Profile entities</description></item>
    /// <item><description><see cref="IProfileCommandService"/> - Handles Profile command operations (Create, Update, Delete)</description></item>
    /// <item><description><see cref="IProfileQueryService"/> - Handles Profile query operations (Read)</description></item>
    /// </list>
    /// All services are registered with scoped lifetime to ensure proper database context management
    /// and request isolation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.AddProfilesContextServices();
    /// </code>
    /// </example>
    public static void AddProfilesContextServices(this WebApplicationBuilder builder)
    {
        // Register repository for data access
        builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
        
        // Register command service for write operations
        builder.Services.AddScoped<IProfileCommandService, ProfileCommandService>();
        
        // Register query service for read operations
        builder.Services.AddScoped<IProfileQueryService, ProfileQueryService>();
    }
}