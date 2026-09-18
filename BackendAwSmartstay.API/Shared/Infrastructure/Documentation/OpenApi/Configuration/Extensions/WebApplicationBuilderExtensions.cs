using Microsoft.OpenApi.Models;
using BackendAwSmartstay.API.Shared.Infrastructure.Documentation.OpenApi.Configuration;

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
                    Description = """
                        REST API of SmartStay, the hotel management platform: accounts and access (authentication,
                        users, audit log), hotels and rooms, bookings and payments, guest and staff profiles, analytics
                        and demo requests from the landing.

                        **Authentication.** Sign in with `POST /api/v1/authentication/sign-in` and send the returned
                        `token` as `Authorization: Bearer <token>`. Access tokens last 30 minutes; with `rememberMe`
                        the response also carries a refresh token for `POST /api/v1/authentication/refresh`.

                        **Errors.** Every error is an RFC 7807 ProblemDetails (`application/problem+json`): read
                        `detail`; validation errors list each invalid field in `errors`.
                        """,
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
            
            // JWT bearer scheme: the "Authorize" button of Swagger UI sends "Authorization: Bearer <token>".
            options.AddSecurityDefinition(BearerSecurityRequirementOperationFilter.SchemeId, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Paste the token returned by POST /api/v1/authentication/sign-in (without the 'Bearer ' prefix).",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            // Shared secret of the external scheduler (only for the scheduled job endpoints).
            options.AddSecurityDefinition(BearerSecurityRequirementOperationFilter.CronKeySchemeId, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "X-Cron-Key",
                Type = SecuritySchemeType.ApiKey,
                Description = "Shared secret of the scheduler (Cron__ApiKey). Only for scheduled job endpoints."
            });

            // Only endpoints that are not [AllowAnonymous] require the bearer token.
            options.OperationFilter<BearerSecurityRequirementOperationFilter>();

            options.EnableAnnotations();

            // XML documentation comments: summaries, remarks, parameters and <example> values of the resources.
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"), includeControllerXmlComments: true);
            options.SupportNonNullableReferenceTypes();
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
