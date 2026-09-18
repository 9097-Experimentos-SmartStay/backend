using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Design-time factory for EF Core migrations when database server is offline.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        // Design-time only (dotnet ef). Uses the same env var as the app; falls back to the local docker compose DB.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = "server=localhost;port=3306;user=smartstay;password=smartstay_dev;database=smartstay;";

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AppDbContext(optionsBuilder.Options);
    }
}
