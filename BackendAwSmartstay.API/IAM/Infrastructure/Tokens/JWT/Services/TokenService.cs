using System.Globalization;
using System.Security.Claims;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Services;

/// <summary>
///     Issues HS256 access tokens whose claims are exactly the ones the JWT bearer handler reads
///     (see <see cref="IamClaimTypes"/>).
/// </summary>
public class TokenService(IOptions<TokenSettings> tokenSettings, TimeProvider timeProvider) : ITokenService
{
    private readonly TokenSettings _tokenSettings = tokenSettings.Value;

    public IssuedAccessToken GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(IamClaimTypes.UserId, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(IamClaimTypes.Username, user.Email.Value),
            new(IamClaimTypes.Email, user.Email.Value),
            new(IamClaimTypes.Role, user.Role.Value),
            new(IamClaimTypes.TokenVersion, user.TokenVersion.ToString(CultureInfo.InvariantCulture)),
            new(IamClaimTypes.EmailVerified, user.EmailVerified ? "true" : "false", ClaimValueTypes.Boolean),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        if (user.HotelId is { } hotelId)
            claims.Add(new Claim(IamClaimTypes.HotelId, hotelId.ToString(CultureInfo.InvariantCulture)));
        if (user.ChainId is { } chainId)
            claims.Add(new Claim(IamClaimTypes.ChainId, chainId.ToString(CultureInfo.InvariantCulture)));

        var (value, expiresAt) = Sign(claims, _tokenSettings.Audience, _tokenSettings.AccessTokenExpirationMinutes);
        return new IssuedAccessToken(value, expiresAt);
    }

    public IssuedMfaChallengeToken GenerateMfaChallengeToken(User user, MfaChallengeKind kind, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(IamClaimTypes.UserId, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(IamClaimTypes.Username, user.Email.Value),
            new(IamClaimTypes.TokenVersion, user.TokenVersion.ToString(CultureInfo.InvariantCulture)),
            new(IamClaimTypes.MfaChallenge, kind == MfaChallengeKind.Enrollment
                ? IamClaimTypes.MfaChallengeEnrollment
                : IamClaimTypes.MfaChallengeVerification),
            new(IamClaimTypes.RememberMe, rememberMe ? "true" : "false", ClaimValueTypes.Boolean),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var (value, expiresAt) = Sign(claims, _tokenSettings.MfaChallengeAudience, _tokenSettings.MfaChallengeTokenExpirationMinutes);
        return new IssuedMfaChallengeToken(kind, value, expiresAt);
    }

    private (string Value, DateTimeOffset ExpiresAt) Sign(List<Claim> claims, string audience, int lifetimeMinutes)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(lifetimeMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            Issuer = _tokenSettings.Issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(_tokenSettings.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        };
        return (new JsonWebTokenHandler().CreateToken(tokenDescriptor), new DateTimeOffset(expires, TimeSpan.Zero));
    }
}
