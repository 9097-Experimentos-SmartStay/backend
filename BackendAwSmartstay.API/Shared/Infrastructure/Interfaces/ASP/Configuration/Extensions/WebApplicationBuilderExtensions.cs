using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Events;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using Microsoft.AspNetCore.Mvc;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Security;

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
        builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Encryption of stored secrets (Data Protection, key ring in the database)
        builder.Services.AddSmartStayDataProtection(builder.Configuration);

        // E-mail port (transactional outbox, delivered in the background) and client URLs used in the links
        builder.Services.AddEmailServices(builder.Configuration);

        // Global error handling: every unhandled exception becomes a ProblemDetails response, and every
        // ProblemDetails (whoever writes it) gets its stable machine-readable code.
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                ProblemCodes.Apply(context);
                if (context.Exception is null) return;
                foreach (var enricher in context.HttpContext.RequestServices.GetServices<IProblemDetailsEnricher>())
                    enricher.Enrich(context);
            };
        });
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Model validation errors remember the code of their rule, for the "violations" of the problem.
        builder.Services.Configure<MvcOptions>(options =>
            options.ModelValidatorProviders.Add(new CodeRecordingModelValidatorProvider()));
    }
}
