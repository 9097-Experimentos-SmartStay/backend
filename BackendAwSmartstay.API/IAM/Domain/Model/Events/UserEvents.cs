using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Events;

// Published language of the IAM context: access-related facts other contexts may react to (the Audit context
// records them, US-03 scenario 4). Every event carries the hotel of the account so readers can scope them.

/// <summary>Why a sign-in attempt was rejected.</summary>
public enum SignInFailureReason
{
    /// <summary>No account uses that e-mail.</summary>
    UnknownEmail,
    /// <summary>The password does not match.</summary>
    WrongPassword,
    /// <summary>The account is temporarily locked after too many failures (US-02 scenario 3).</summary>
    AccountLocked,
    /// <summary>The account is deactivated (US-03 scenario 3).</summary>
    AccountDeactivated,
    /// <summary>The e-mail of the account is not verified yet (US-01).</summary>
    EmailNotVerified
}

/// <summary>A user signed in successfully.</summary>
public sealed record UserSignedInEvent(int UserId, string Email, int? HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A sign-in attempt was rejected. <paramref name="UserId"/> is null when the e-mail is unknown.</summary>
public sealed record SignInFailedEvent(int? UserId, string Email, int? HotelId, SignInFailureReason Reason, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>Too many consecutive failures: the account is locked until <paramref name="LockedUntil"/> (US-02 scenario 3).</summary>
public sealed record UserLockedOutEvent(int UserId, string Email, int? HotelId, DateTimeOffset LockedUntil, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A user signed out (their remembered session was revoked).</summary>
public sealed record UserSignedOutEvent(int UserId, string Email, int? HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A user set a new password through the recovery link (US-04).</summary>
public sealed record UserPasswordResetEvent(int UserId, string Email, int? HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A user changed their password knowing the current one.</summary>
public sealed record UserPasswordChangedEvent(int UserId, string Email, int? HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>
///     An account was created: by self-registration (<paramref name="CreatedByUserId"/> null) or by an administrator.
/// </summary>
public sealed record UserCreatedEvent(int UserId, string Email, int? HotelId, string Role, int? CreatedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>An administrator changed the role of a user: every session of the user ended (US-03 scenario 2).</summary>
public sealed record UserRoleChangedEvent(int UserId, string Email, int? HotelId, string PreviousRole, string NewRole, int? ChangedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>
///     The hotel or chain of a user changed (an administrator reassigned them, or a hotel administrator registered
///     their hotel): every session of the user ended.
/// </summary>
public sealed record UserAssignmentChangedEvent(int UserId, string Email, int? PreviousHotelId, int? HotelId,
    int? PreviousChainId, int? ChainId, int? ChangedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>An administrator deactivated a user: they lose access, their history is kept.</summary>
public sealed record UserDeactivatedEvent(int UserId, string Email, int? HotelId, int? DeactivatedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>An administrator reactivated a user.</summary>
public sealed record UserActivatedEvent(int UserId, string Email, int? HotelId, int? ActivatedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>How a user proved the second factor.</summary>
public enum MfaMethod
{
    /// <summary>A 6-digit code of the authenticator app.</summary>
    AuthenticatorCode,
    /// <summary>A one-time recovery code.</summary>
    RecoveryCode
}

/// <summary>A user enrolled an authenticator app: two-factor authentication is on (US-52 scenario 1).</summary>
public sealed record MfaEnabledEvent(int UserId, string Email, int? HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A user proved the second factor while signing in (US-52 scenarios 2 and 3).</summary>
public sealed record MfaVerifiedEvent(int UserId, string Email, int? HotelId, MfaMethod Method, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A wrong, reused or malformed second-factor code was presented (counts toward the temporary lock).</summary>
public sealed record MfaVerificationFailedEvent(int UserId, string Email, int? HotelId, MfaMethod Method, string Reason, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A one-time recovery code was used (US-52 scenario 3); <paramref name="RemainingCodes"/> are left.</summary>
public sealed record MfaRecoveryCodeUsedEvent(int UserId, string Email, int? HotelId, int RemainingCodes, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>An administrator reset the second factor of a user, who must enroll again (US-52 scenario 4).</summary>
public sealed record MfaResetEvent(int UserId, string Email, int? HotelId, int? ResetByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>A user closed every session on every device.</summary>
public sealed record UserSignedOutEverywhereEvent(int UserId, string Email, int? HotelId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);
