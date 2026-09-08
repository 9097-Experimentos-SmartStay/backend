using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAwSmartstay.API.shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Provides extension methods for <see cref="WebApplicationBuilder"/> to configure persistence-related services.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public static void AddDatabaseConfigurationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");
            }
            if (builder.Environment.IsDevelopment())
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            }
            else 
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .LogTo(Console.WriteLine, LogLevel.Error)
                    .EnableDetailedErrors();
            }
        });
    }
}