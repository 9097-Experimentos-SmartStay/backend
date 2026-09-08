using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAwSmartstay.API.shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class WebApplicationExtensions
{
    public static void EnsureDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();
        var context = services.GetRequiredService<AppDbContext>();
        
        try
        {
            logger.LogInformation("Verificando y creando la base de datos si no existe...");
            var canConnect = context.Database.CanConnect();
            logger.LogInformation("¿Puede conectar a la base de datos? {CanConnect}", canConnect);
            
            if (!canConnect)
            {
                logger.LogInformation("La base de datos no existe. Creándola...");
            }
            
            context.Database.EnsureCreated();
            logger.LogInformation("Base de datos verificada/creada exitosamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear la base de datos: {Message}", ex.Message);
            logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            throw;
        }
    }
}