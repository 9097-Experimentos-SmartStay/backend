using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     Single-use token sent by e-mail as a link: e-mail verification (US-01) or password reset (US-04).
///     Only a hash of the token is stored; the token itself only travels in the e-mail.
/// </summary>
public class AccountToken
{
    /// <summary>EF Core constructor.</summary>
    protected AccountToken()
    {
        TokenHash = string.Empty;
    }

    private AccountToken(int userId, AccountTokenPurpose purpose, string tokenHash, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        UserId = userId;
        Purpose = purpose;
        TokenHash = tokenHash;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public AccountTokenPurpose Purpose { get; private set; }

    /// <summary>SHA-256 of the token (hex). The token is never stored.</summary>
    public string TokenHash { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>When the link was used; a used link cannot be used again.</summary>
    public DateTimeOffset? ConsumedAt { get; private set; }

    /// <summary>When the link was superseded by a newer one.</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsOutstanding => ConsumedAt is null && RevokedAt is null;

    /// <summary>Issues a token for <paramref name="userId"/> valid for <paramref name="lifetime"/>.</summary>
    public static AccountToken Issue(int userId, AccountTokenPurpose purpose, string tokenHash, DateTimeOffset now, TimeSpan lifetime)
    {
        if (userId <= 0) throw new DomainValidationException("An account token must belong to a user.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainValidationException("An account token needs a hash.");
        if (lifetime <= TimeSpan.Zero) throw new DomainValidationException("An account token lifetime must be positive.");
        return new AccountToken(userId, purpose, tokenHash, now, now + lifetime);
    }

    /// <summary>Uses the link. Single use: a second use, or a superseded link, is invalid; an old link is expired.</summary>
    /// <exception cref="InvalidAccountTokenException">Already used or superseded.</exception>
    /// <exception cref="AccountTokenExpiredException">Older than its lifetime (US-04 scenario 3).</exception>
    public void Consume(DateTimeOffset now)
    {
        if (!IsOutstanding) throw new InvalidAccountTokenException(Purpose);
        if (now >= ExpiresAt) throw new AccountTokenExpiredException(Purpose);
        ConsumedAt = now;
    }

    /// <summary>Supersedes the link (a newer one was sent, or the purpose is already fulfilled).</summary>
    public void Revoke(DateTimeOffset now)
    {
        if (IsOutstanding) RevokedAt = now;
    }
}
