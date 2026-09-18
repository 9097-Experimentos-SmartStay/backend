using BackendAwSmartstay.API.Accommodations.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Bookings.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Payments.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Mediator.Cortex.Configuration.Extensions;
using BackendAwSmartstay.API.IAM.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.IAM.Infrastructure.Extensions;
using BackendAwSmartstay.API.Profiles.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.Analytics.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Controllers.Authorization;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention())
);

// Database
builder.AddDatabaseConfigurationServices();

// OpenAPI / Swagger
builder.AddOpenApiConfigurationServices();

// CORS
builder.AddCorsServices();

 
// DI / Contextos
builder.AddSharedContextServices();
builder.AddAccommodationsContextServices();
builder.AddBookingsContextServices();
builder.AddPaymentsContextServices();
builder.AddIamContextServices();
builder.AddProfilesContextServices();
builder.AddAnalyticsContextServices();
builder.AddIoTEmulatorServices();

// Mediator for Services
builder.AddCortexMediatorServices();

// New implementation - Health Checks
builder.Services.AddHealthChecks()
    .AddMySql(builder.Configuration.GetConnectionString("DefaultConnection")!, 
        name: "mysql-db-check", 
        tags: new[] { "database" });

// Optional analytics cache lab: Redis + ActiveMQ fallback (only when configured)
builder.AddAnalyticsCacheServices();

// Rate limiting of the anonymous endpoints, per client IP
builder.Services.AddSmartStayRateLimiting(builder.Configuration);

var app = builder.Build();

// --- Database initialization: migrations + seed. Fail fast: never start with a broken schema ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>(); 
        
        // Ejecuta las migraciones pendientes en la nube o local de forma automática
        if (context.Database.IsRelational())
        {
            logger.LogInformation("Applying pending database migrations...");
            await context.Database.MigrateAsync();
        }
        
        await app.SeedDatabaseAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration or seeding failed at startup. The application will stop.");
        throw;
    }
}

// Pipeline de Middlewares (HTTP request pipeline)
// Global exception handler first, so errors from every later middleware become ProblemDetails
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseOpenApiConfiguration();
// CORS (origins from Cors__AllowedOrigins)
app.UseCorsPolicy();
// user httpRedirection
app.UseHttpsRedirection();

app.UseRateLimiter();

// Native ASP.NET Core authentication (JWT bearer) and authorization (fallback policy: authenticated user)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapSwagger().AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

/// <summary>Entry point, exposed for integration tests (WebApplicationFactory).</summary>
public partial class Program;
