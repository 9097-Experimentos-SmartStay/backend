using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Mediator.Cortex.Configuration.Extensions;

/// <summary>
/// Provides extension methods for configuring Cortex Mediator services in the web application builder.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Cortex Mediator services to the dependency injection container.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    public static void AddCortexMediatorServices(this WebApplicationBuilder builder)
    {
        // Add Mediator Injection Configuration
        builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));

        // Add Cortex Mediator for Event Handling
        builder.Services.AddCortexMediator(
            builder.Configuration,
            [typeof(Program)], options =>
            {
                options.AddOpenCommandPipelineBehavior(typeof(LoggingCommandBehavior<>));
                //options.AddDefaultBehaviors();
            });
    }
}
