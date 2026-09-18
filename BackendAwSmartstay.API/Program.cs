using BackendAwSmartstay.API.Accommodations.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Audit.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.DemoData.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Marketing.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Media.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Authentication.ScheduledJobs;
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
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ReverseProxy;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
    // Validation errors are keyed by the JSON (camelCase) property names the clients send.
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
});

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
builder.AddAuditContextServices();
builder.AddMarketingContextServices();
builder.AddMediaContextServices();
builder.AddIoTEmulatorServices();

// Opt-in demo dataset (DemoData__Enabled), created after the migrations
builder.AddDemoDataServices();

// Mediator for Services
builder.AddCortexMediatorServices();

// New implementation - Health Checks
builder.Services.AddHealthChecks()
    .AddMySql(builder.Configuration.GetConnectionString("DefaultConnection")!, 
        name: "mysql-db-check", 
        tags: new[] { "database" });

// Optional analytics cache lab: Redis + ActiveMQ fallback (only when configured)
builder.AddAnalyticsCacheServices();

// X-Cron-Key authentication of the external scheduler (scheduled jobs)
builder.Services.AddScheduledJobsAuthentication(builder.Configuration);

// Real client IP behind Cloudflare + Render's load balancer (X-Forwarded-For from trusted proxies only)
builder.Services.AddSmartStayForwardedHeaders(builder.Configuration);

// Rate limiting of the anonymous endpoints, per client IP
builder.Services.AddSmartStayRateLimiting(builder.Configuration);

var app = builder.Build();

// --- Database initialization: migrations + seed (+ demo data when enabled). Fail fast: never start with a broken schema ---
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
        await app.SeedDemoDataAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration or seeding failed at startup. The application will stop.");
        throw;
    }
}

// Pipeline de Middlewares (HTTP request pipeline)
// Forwarded headers first: the client IP and scheme must be resolved before anything reads them
// (rate limiter partitions, audit log, error responses).
app.UseSmartStayForwardedHeaders();
// Global exception handler first, so errors from every later middleware become ProblemDetails
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseOpenApiConfiguration();
// CORS (origins from Cors__AllowedOrigins)
app.UseCorsPolicy();
// HTTPS redirection only where Kestrel itself serves HTTPS (dotnet run, Development). Behind Render's proxy TLS ends
// at the edge, which already redirects http→https; the container only listens on http (no https port to redirect
// to, hence the old "Failed to determine the https port" warning) and the forwarded headers set the https scheme.
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// Native ASP.NET Core authentication (JWT bearer) and authorization (fallback policy: authenticated user).
// The rate limiter runs after authentication so per-user policies (media uploads) see the signed-in user.
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();
app.MapApiDocumentation();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

/// <summary>Entry point, exposed for integration tests (WebApplicationFactory).</summary>
public partial class Program;
