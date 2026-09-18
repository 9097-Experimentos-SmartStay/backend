using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     "Remember me" session (US-02 scenario 4): a long-lived, single-use token exchanged for a new access token.
///     <list type="bullet">
///         <item>Only its SHA-256 hash is stored.</item>
///         <item>Rotation: every use revokes it and issues its successor in the same <see cref="FamilyId"/>
///         (one family = one remembered session on one device); the expiry slides with each use.</item>
///         <item>Reuse detection: presenting a token that was already rotated means it was copied, so the whole
///         family is revoked and the user must sign in again.</item>
///         <item>It belongs to one session generation of the user (<see cref="TokenVersion"/>): a password change or
///         reset, or a deactivation, invalidates it.</item>
///     </list>
/// </summary>
public class RefreshToken
{
    /// <summary>EF Core constructor.</summary>
    protected RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(int userId, Guid familyId, int tokenVersion, string tokenHash, DateTimeOffset now, TimeSpan lifetime)
    {
        if (userId <= 0) throw new DomainValidationException(IamErrorCodes.InternalInvariant, "A refresh token must belong to a user.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainValidationException(IamErrorCodes.InternalInvariant, "A refresh token needs a hash.");
        if (lifetime <= TimeSpan.Zero) throw new DomainValidationException(IamErrorCodes.InternalInvariant, "A refresh token lifetime must be positive.");

        UserId = userId;
        FamilyId = familyId;
        TokenVersion = tokenVersion;
        TokenHash = tokenHash;
        IssuedAt = now;
        ExpiresAt = now + lifetime;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }

    /// <summary>The remembered session this token belongs to (all its rotations share it).</summary>
    public Guid FamilyId { get; private set; }

    /// <summary>The user's session generation when the family was started.</summary>
    public int TokenVersion { get; private set; }

    /// <summary>SHA-256 of the token (hex). The token is never stored.</summary>
    public string TokenHash { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public RefreshTokenRevocationReason? RevocationReason { get; private set; }

    /// <summary>Starts a remembered session for <paramref name="user"/>.</summary>
    public static RefreshToken StartSession(User user, string tokenHash, DateTimeOffset now, TimeSpan lifetime) =>
        new(user.Id, Guid.NewGuid(), user.TokenVersion, tokenHash, now, lifetime);

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && now < ExpiresAt;

    /// <summary>True when the token was already exchanged: presenting it again is a replay.</summary>
    public bool WasRotated => RevocationReason == RefreshTokenRevocationReason.Rotated;

    /// <summary>True when the token still belongs to the user's current session generation.</summary>
    public bool BelongsToCurrentSessionOf(User user) => user.Id == UserId && user.TokenVersion == TokenVersion;

    /// <summary>Exchanges this token for its successor (same session, new expiry).</summary>
    /// <exception cref="InvalidRefreshTokenException">The token is not active any more.</exception>
    public RefreshToken Rotate(string successorHash, DateTimeOffset now, TimeSpan lifetime)
    {
        if (!IsActive(now)) throw new InvalidRefreshTokenException();
        Revoke(RefreshTokenRevocationReason.Rotated, now);
        return new RefreshToken(UserId, FamilyId, TokenVersion, successorHash, now, lifetime);
    }

    /// <summary>Revokes the token (no-op when it is already revoked).</summary>
    public void Revoke(RefreshTokenRevocationReason reason, DateTimeOffset now)
    {
        if (RevokedAt is not null) return;
        RevokedAt = now;
        RevocationReason = reason;
    }
}
