using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Seed;

/// <summary>
///     Creates the initial chain administrator when <see cref="InitialChainAdminSettings"/> are provided. Its password
///     goes through the same policy as any other password: an unacceptable one stops the startup.
/// </summary>
public static class IamSeeder
{
    private const string PasswordSettingKey = $"{InitialChainAdminSettings.SectionName}:Password";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(IamSeeder));
        var settings = services.GetRequiredService<IOptions<InitialChainAdminSettings>>().Value;

        if (!settings.IsConfigured)
        {
            logger.LogWarning("IamSeeder: InitialChainAdmin credentials not provided; skipping seed.");
            return;
        }

        var userRepository = services.GetRequiredService<IUserRepository>();
        var email = new Email(settings.LoginEmail!);
        if (await userRepository.ExistsByEmailAsync(email))
        {
            logger.LogInformation("IamSeeder: user '{Email}' already exists; skipping seed.", email.Value);
            return;
        }

        await services.GetRequiredService<NewPasswordValidator>().EnsureAcceptableAsync(
            settings.Password, new Role(UserRoles.ChainAdmin), email, PasswordSettingKey);

        var hashingService = services.GetRequiredService<IHashingService>();
        var user = new User(email.Value, hashingService.HashPassword(settings.Password!), UserRoles.ChainAdmin,
            hotelId: settings.HotelId);
        // The operator provides this address: it does not need the verification link.
        user.VerifyEmail(DateTimeOffset.UtcNow);

        await userRepository.AddAsync(user);
        await services.GetRequiredService<IUnitOfWork>().CompleteAsync();
        logger.LogInformation("IamSeeder: created initial chain admin '{Email}' (hotel {HotelId}).",
            email.Value, settings.HotelId);
    }
}
