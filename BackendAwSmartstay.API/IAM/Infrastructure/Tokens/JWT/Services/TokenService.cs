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

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(IamClaimTypes.UserId, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(IamClaimTypes.Username, user.Email.Value),
            new(IamClaimTypes.Email, user.Email.Value),
            new(IamClaimTypes.Role, user.Role.Value),
            new(IamClaimTypes.TokenVersion, user.TokenVersion.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        if (user.HotelId is { } hotelId)
            claims.Add(new Claim(IamClaimTypes.HotelId, hotelId.ToString(CultureInfo.InvariantCulture)));
        if (user.ChainId is { } chainId)
            claims.Add(new Claim(IamClaimTypes.ChainId, chainId.ToString(CultureInfo.InvariantCulture)));

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddHours(_tokenSettings.ExpirationInHours),
            Issuer = _tokenSettings.Issuer,
            Audience = _tokenSettings.Audience,
            SigningCredentials = new SigningCredentials(_tokenSettings.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}
