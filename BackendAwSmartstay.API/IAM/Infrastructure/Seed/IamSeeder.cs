using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Seed;

public class IamSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<IamSeeder>>();
        try
        {
            var config = services.GetRequiredService<IConfiguration>();
            var username = config["InitialChainAdmin:Username"] ?? Environment.GetEnvironmentVariable("INITIAL_CHAIN_ADMIN_USERNAME");
            var password = config["InitialChainAdmin:Password"] ?? Environment.GetEnvironmentVariable("INITIAL_CHAIN_ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("IamSeeder: Initial chain admin credentials not provided; skipping seed.");
                return;
            }

            var userRepository = services.GetRequiredService<IUserRepository>();
            var hashingService = services.GetRequiredService<IHashingService>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            if (await userRepository.ExistsByUsernameAsync(username))
            {
                logger.LogWarning("IamSeeder: User '{Username}' already exists; skipping seed.", username);
                return;
            }

            var hashed = hashingService.HashPassword(password);
            var user = new User(username, hashed, UserRoles.ChainAdmin, hotelId: 1);

            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
            logger.LogInformation("IamSeeder: Created initial chain admin user '{Username}' with HotelId=1.", username);
        }
        catch (Exception e)
        {
            logger.LogError(e, "IamSeeder: Seed failed.");
            throw;
        }
    }
}