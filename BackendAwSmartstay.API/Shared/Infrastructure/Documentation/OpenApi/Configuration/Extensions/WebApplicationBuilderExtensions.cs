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
    /// Configures CORS policy to allow all origins, methods, and headers.
    /// </summary>
    /// <param name="builder">The web application builder instance.</param>
    /// <remarks>
    /// Warning: This policy allows unrestricted cross-origin access. 
    /// Consider restricting origins in production environments.
    /// </remarks>
    public static void AddCorsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins("https://smartstay-3cffc.web.app")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}