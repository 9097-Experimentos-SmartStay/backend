using System.Text;
using BackendAwSmartstay.API.IAM.Application.ACL.Services;
using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Application.Internal.QueryServices;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddIamContextServices(this WebApplicationBuilder builder)
    {
        // TokenSettings Configuration (fail fast: the signing secret must come from configuration/env vars)

        var tokenSettingsSection = builder.Configuration.GetSection("TokenSettings");
        var tokenSecret = tokenSettingsSection["Secret"];
        if (string.IsNullOrWhiteSpace(tokenSecret))
            throw new InvalidOperationException(
                "TokenSettings:Secret is not configured. Set the 'TokenSettings__Secret' environment variable " +
                "(at least 32 random characters).");
        if (Encoding.UTF8.GetByteCount(tokenSecret) < TokenSettings.MinimumSecretLength)
            throw new InvalidOperationException(
                $"TokenSettings:Secret is too short. HS256 requires at least {TokenSettings.MinimumSecretLength} bytes.");

        builder.Services.Configure<TokenSettings>(tokenSettingsSection);

        // IAM Bounded Context Injection Configuration

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserCommandService, UserCommandService>();
        builder.Services.AddScoped<IUserQueryService, UserQueryService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IHashingService, HashingService>();
        builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();
        
        builder.Services.AddScoped<IRoleAuthorizationService, RoleAuthorizationService>();
        builder.Services.AddScoped<IUserScopeService, UserScopeService>();
    }
}