using BackendAwSmartstay.API.IAM.Application.Internal.Configuration;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>A link token ready to be e-mailed once the unit of work commits.</summary>
public sealed record PendingAccountToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
///     Issues single-use account tokens: supersedes the outstanding ones of the same purpose (only the newest link
///     works) and adds the new one to the unit of work. The caller commits and then e-mails the link.
/// </summary>
public class AccountTokenIssuer(
    IAccountTokenRepository accountTokenRepository,
    ISecureTokenGenerator secureTokenGenerator,
    IOptions<AccountSecuritySettings> settings,
    TimeProvider timeProvider)
{
    public async Task<PendingAccountToken> IssueAsync(User user, AccountTokenPurpose purpose)
    {
        var now = timeProvider.GetUtcNow();
        foreach (var outstanding in await accountTokenRepository.ListOutstandingAsync(user.Id, purpose))
            outstanding.Revoke(now);

        var lifetime = purpose == AccountTokenPurpose.PasswordReset
            ? settings.Value.PasswordResetTokenLifetime
            : settings.Value.EmailVerificationTokenLifetime;
        var token = secureTokenGenerator.Generate();
        var accountToken = AccountToken.Issue(user.Id, purpose, token.Hash, now, lifetime);
        await accountTokenRepository.AddAsync(accountToken);
        return new PendingAccountToken(token.Value, accountToken.ExpiresAt);
    }

    /// <summary>Uses a link token: throws when it is unknown, used, superseded or expired.</summary>
    public async Task<AccountToken> ConsumeAsync(string tokenValue, AccountTokenPurpose purpose)
    {
        var accountToken = await accountTokenRepository.FindByHashAsync(purpose, secureTokenGenerator.Hash(tokenValue))
                           ?? throw new Domain.Model.Exceptions.InvalidAccountTokenException(purpose);
        accountToken.Consume(timeProvider.GetUtcNow());
        return accountToken;
    }
}
