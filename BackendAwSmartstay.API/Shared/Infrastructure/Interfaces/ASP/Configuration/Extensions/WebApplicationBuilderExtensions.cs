using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
/// Provides extension methods for configuring shared services in the web application builder.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds shared context services to the dependency injection container.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    public static void AddSharedContextServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
