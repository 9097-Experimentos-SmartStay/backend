using BackendAwSmartstay.API.Accommodations.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Bookings.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Payments.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Mediator.Cortex.Configuration.Extensions;
using BackendAwSmartstay.API.IAM.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using BackendAwSmartstay.API.IAM.Infrastructure.Extensions;
using BackendAwSmartstay.API.Profiles.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.Analytics.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.AspNetCore.RateLimiting;
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

// Mediator for Services
builder.AddCortexMediatorServices();

// New implementation - Health Checks
builder.Services.AddHealthChecks()
    .AddMySql(builder.Configuration.GetConnectionString("DefaultConnection")!, 
        name: "mysql-db-check", 
        tags: new[] { "database" });

// Optional analytics cache lab: Redis + ActiveMQ fallback (only when configured)
builder.AddAnalyticsCacheServices();

// Rate Limiting Configuration
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthLimiter", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10; // Máximo 10 intentos por minuto
        opt.QueueLimit = 0;   // Rechazo inmediato sin encolar
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
    
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

app.UseRequestAuthorization();

app.MapControllers();
// maping health checks endpoint
app.MapHealthChecks("/health");

app.Run();
