using BackendAwSmartstay.API.IAM.Application.ACL.Services;
using BackendAwSmartstay.API.IAM.Application.Internal.Configuration;
using BackendAwSmartstay.API.IAM.Infrastructure.Notifications;
using BackendAwSmartstay.API.IAM.Infrastructure.Passwords;
using BackendAwSmartstay.API.IAM.Interfaces.REST.ExceptionHandling;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.Opaque;
using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Application.Internal.QueryServices;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Authentication;
using BackendAwSmartstay.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Infrastructure.Seed;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddIamContextServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddIamAuthentication(builder.Configuration);
        builder.Services.AddIamAuthorization();

        builder.Services.AddOptions<InitialChainAdminSettings>()
            .Bind(builder.Configuration.GetSection(InitialChainAdminSettings.SectionName))
            // Legacy variable names kept for existing deployments.
            .PostConfigure(settings =>
            {
                if (string.IsNullOrWhiteSpace(settings.LoginEmail))
                    settings.Email = Environment.GetEnvironmentVariable("INITIAL_CHAIN_ADMIN_USERNAME");
                if (string.IsNullOrWhiteSpace(settings.Password))
                    settings.Password = Environment.GetEnvironmentVariable("INITIAL_CHAIN_ADMIN_PASSWORD");
            });

        // IAM Bounded Context Injection Configuration
        builder.Services.AddOptions<AccountSecuritySettings>()
            .Bind(builder.Configuration.GetSection(AccountSecuritySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Password policy (NIST SP 800-63B-4) with the breached password lookup (HIBP range API, k-anonymity)
        builder.Services.AddOptions<PasswordPolicySettings>()
            .Bind(builder.Configuration.GetSection(PasswordPolicySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddHttpClient<IBreachedPasswordChecker, PwnedPasswordsChecker>((services, client) =>
        {
            var settings = services.GetRequiredService<IOptions<PasswordPolicySettings>>().Value;
            client.BaseAddress = new Uri(settings.PwnedPasswordsApiBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(settings.PwnedPasswordsTimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SmartStay-API/1.0");
        });
        builder.Services.AddScoped<NewPasswordValidator>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IAccountTokenRepository, AccountTokenRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<AccountTokenIssuer>();
        builder.Services.AddScoped<IAuthenticationCommandService, AuthenticationCommandService>();
        builder.Services.AddScoped<IUserCommandService, UserCommandService>();
        builder.Services.AddScoped<IUserQueryService, UserQueryService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IHashingService, HashingService>();
        builder.Services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        builder.Services.AddScoped<IAccountNotificationService, AccountEmailNotificationService>();
        builder.Services.AddSingleton<IProblemDetailsEnricher, IamProblemDetailsEnricher>();
        builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

        builder.Services.AddScoped<IRoleAuthorizationService, RoleAuthorizationService>();
        builder.Services.AddScoped<IUserScopeService, UserScopeService>();
    }

    /// <summary>
    ///     Native ASP.NET Core authentication: JWT bearer tokens signed with <see cref="TokenSettings"/>.
    ///     The settings are validated when the host starts (the app does not boot without a strong secret).
    /// </summary>
    public static IServiceCollection AddIamAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TokenSettings>()
            .Bind(configuration.GetSection(TokenSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IamJwtBearerEvents>();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureIamJwtBearerOptions>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        return services;
    }

    /// <summary>
    ///     Native ASP.NET Core authorization: every endpoint requires an authenticated user unless it is
    ///     explicitly marked <see cref="AllowAnonymousAttribute"/>; capabilities are the policies of
    ///     <see cref="Policies"/>.
    /// </summary>
    public static IServiceCollection AddIamAuthorization(this IServiceCollection services)
    {
        var authenticatedUser = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(authenticatedUser)
            .SetFallbackPolicy(authenticatedUser)
            .AddSmartStayPolicies();

        return services;
    }
}
