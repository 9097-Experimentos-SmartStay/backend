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
///     asked (US-02 scenario 4), commits and issues the access token. Also issues the new credentials of a user
///     whose permissions changed at their own request.
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
        return await IssueAsync(user, rememberMe, now, recoveryCodes);
    }

    /// <summary>
    ///     OWASP "issue new credentials on privilege change": the user's previous sessions already ended (new session
    ///     generation) because of a change they asked for themselves, so they get a fresh session right away with the
    ///     new permissions instead of signing in again. It is remembered only when the session they used was.
    ///     Not a sign-in: the sign-in history is not touched.
    /// </summary>
    public Task<AuthenticationResult> ReissueSessionAsync(User user, bool remembered, DateTimeOffset now) =>
        IssueAsync(user, remembered, now, recoveryCodes: null);

    private async Task<AuthenticationResult> IssueAsync(User user, bool rememberMe, DateTimeOffset now,
        IReadOnlyList<string>? recoveryCodes)
    {
        IssuedRefreshToken? refreshToken = null;
        Guid? rememberedSessionId = null;
        if (rememberMe)
        {
            var token = secureTokenGenerator.Generate();
            var session = RefreshToken.StartSession(user, token.Hash, now, settings.Value.RefreshTokenLifetime);
            await refreshTokenRepository.AddAsync(session);
            refreshToken = new IssuedRefreshToken(token.Value, session.ExpiresAt);
            rememberedSessionId = session.FamilyId;
        }

        await unitOfWork.CompleteAsync();
        var accessToken = tokenService.GenerateToken(user, rememberedSessionId);
        return new AuthenticationResult(user, accessToken.Value, accessToken.ExpiresAt, refreshToken, RecoveryCodes: recoveryCodes);
    }
}
