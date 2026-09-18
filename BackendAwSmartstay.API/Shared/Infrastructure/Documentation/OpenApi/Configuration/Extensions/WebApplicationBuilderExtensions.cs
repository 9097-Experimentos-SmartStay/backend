using Microsoft.OpenApi.Models;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI documentation and CORS policies.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures OpenAPI/Swagger documentation services with JWT authentication support.
    /// </summary>
    /// <param name="builder">The web application builder instance.</param>
    public static void AddOpenApiConfigurationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Configure API documentation metadata
            options.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "SmartStay Platform API",
                    Version = "v1",
                    Description = "Hotel Management System API - Accommodations, Bookings and Payments",
                    TermsOfService = new Uri("https://smartstay.com/tos"),
                    Contact = new OpenApiContact
                    {
                        Name = "SmartStay",
                        Email = "contact@smartstay.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
                    }
                });
            
            // Configure JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter JWT token with Bearer prefix",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            
            // Apply JWT authentication globally
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            options.EnableAnnotations();
        });
    }

    /// <summary>
    /// Name of the CORS policy applied by the pipeline.
    /// </summary>
    public const string CorsPolicyName = "AllowFrontend";

    /// <summary>
    /// Configures the CORS policy from <c>Cors:AllowedOrigins</c>
    /// (env var <c>Cors__AllowedOrigins</c>, comma-separated, or a JSON array in appsettings).
    /// </summary>
    /// <param name="builder">The web application builder instance.</param>
    public static void AddCorsServices(this WebApplicationBuilder builder)
    {
        var allowedOrigins = GetAllowedOrigins(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // No origin configured: browsers from other origins are rejected.
                    return;
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    /// <summary>
    /// Reads allowed origins either as a single comma/semicolon separated string or as an array section.
    /// </summary>
    public static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var section = configuration.GetSection("Cors:AllowedOrigins");
        var rawValues = section.GetChildren().Any()
            ? section.GetChildren().Select(child => child.Value)
            : new[] { section.Value };

        return rawValues
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split(new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(origin => origin.TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
