using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAwSmartstay.API.shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Provides extension methods for <see cref="WebApplicationBuilder"/> to configure persistence-related services.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    ///     MySQL server version assumed when <c>Database:MySqlServerVersion</c> is not configured.
    ///     Pinning the version avoids <c>ServerVersion.AutoDetect</c>, which opens a DB connection
    ///     every time the context options are built and fails when the DB is not reachable yet.
    /// </summary>
    private const string DefaultMySqlServerVersion = "8.0.36";

    public static void AddDatabaseConfigurationServices(this WebApplicationBuilder builder)
    {
        // Fail fast: resolve and validate the connection string once, at startup.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Set the 'ConnectionStrings__DefaultConnection' environment variable " +
                "(e.g. server=host;port=3306;user=...;password=...;database=...;).");
        }

        var versionText = builder.Configuration["Database:MySqlServerVersion"];
        var serverVersion = new MySqlServerVersion(
            Version.Parse(string.IsNullOrWhiteSpace(versionText) ? DefaultMySqlServerVersion : versionText));

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.UseMySql(connectionString, serverVersion)
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            }
            else
            {
                options.UseMySql(connectionString, serverVersion)
                    .LogTo(Console.WriteLine, LogLevel.Error)
                    .EnableDetailedErrors();
            }
        });
    }
}
