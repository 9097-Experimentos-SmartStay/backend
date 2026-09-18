using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     A one-time recovery code of a user's second factor (US-52 scenario 3). Only a password-grade hash is stored
///     (NIST SP 800-63B-4: look-up secrets under 112 bits of entropy are hashed with a password hashing scheme).
///     A used code can never be used again.
/// </summary>
public class MfaRecoveryCode
{
    /// <summary>EF Core constructor.</summary>
    protected MfaRecoveryCode()
    {
        CodeHash = string.Empty;
    }

    private MfaRecoveryCode(int userId, string codeHash, DateTimeOffset createdAt)
    {
        if (userId <= 0) throw new DomainValidationException("A recovery code must belong to a user.");
        if (string.IsNullOrWhiteSpace(codeHash)) throw new DomainValidationException("A recovery code needs a hash.");
        UserId = userId;
        CodeHash = codeHash;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string CodeHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    public bool IsUsed => UsedAt is not null;

    public static MfaRecoveryCode Issue(int userId, string codeHash, DateTimeOffset now) => new(userId, codeHash, now);

    /// <summary>Uses the code (single use).</summary>
    public void Redeem(DateTimeOffset now)
    {
        if (IsUsed) throw new BusinessRuleViolationException("This recovery code was already used.");
        UsedAt = now;
    }
}
