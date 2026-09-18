using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Authentication;

/// <summary>
///     Configures the native JWT bearer handler from the validated <see cref="TokenSettings"/>
///     (options pattern, so the secret is never read before validation).
/// </summary>
public class ConfigureIamJwtBearerOptions(IOptions<TokenSettings> tokenSettings)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme) return;

        var settings = tokenSettings.Value;

        // Keep the short JWT claim names (sub, role, unique_name...) instead of the legacy WS-* URIs.
        options.MapInboundClaims = false;
        options.EventsType = typeof(IamJwtBearerEvents);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = settings.CreateSigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
            NameClaimType = IamClaimTypes.Username,
            RoleClaimType = IamClaimTypes.Role
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);
}
