using BackendAwSmartstay.API.IAM.Application.Internal.Configuration;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Grants access once every factor is proven: records the successful sign-in, starts a remembered session when
///     asked (US-02 scenario 4), commits and issues the access token.
/// </summary>
public class SessionIssuer(
    IRefreshTokenRepository refreshTokenRepository,
    ISecureTokenGenerator secureTokenGenerator,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IOptions<AccountSecuritySettings> settings)
{
    public async Task<AuthenticationResult> StartSessionAsync(User user, bool rememberMe, DateTimeOffset now,
        IReadOnlyList<string>? recoveryCodes = null)
    {
        user.RegisterSuccessfulSignIn(now);
        IssuedRefreshToken? refreshToken = null;
        if (rememberMe)
        {
            var token = secureTokenGenerator.Generate();
            var session = RefreshToken.StartSession(user, token.Hash, now, settings.Value.RefreshTokenLifetime);
            await refreshTokenRepository.AddAsync(session);
            refreshToken = new IssuedRefreshToken(token.Value, session.ExpiresAt);
        }

        await unitOfWork.CompleteAsync();
        var accessToken = tokenService.GenerateToken(user);
        return new AuthenticationResult(user, accessToken.Value, accessToken.ExpiresAt, refreshToken, RecoveryCodes: recoveryCodes);
    }
}
