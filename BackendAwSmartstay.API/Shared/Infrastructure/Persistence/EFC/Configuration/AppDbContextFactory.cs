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

        optionsBuilder.UseMySql("server=localhost;user=root;password=12345678;database=backend-smartstay-db;", serverVersion);

        return new AppDbContext(optionsBuilder.Options);
    }
}
