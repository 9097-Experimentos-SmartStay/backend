using BackendAwSmartstay.API.IAM.Infrastructure.Seed;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Extensions;

public static class WebApplicationIamExtensions
{
    /// <summary>
    /// Seeds initial IAM data (chain admin user) if configured.
    /// Follows the same extension pattern as EnsureDatabaseCreated().
    /// </summary>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<IamSeeder>>();
        try
        {
            await IamSeeder.SeedAsync(services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding IAM data.");
            throw;
        }
    }
}




