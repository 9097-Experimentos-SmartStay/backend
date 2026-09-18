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
    AccountDeactivated
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

/// <summary>An administrator changed the role of a user (effective on the user's next request).</summary>
public sealed record UserRoleChangedEvent(int UserId, string Email, int? HotelId, string PreviousRole, string NewRole, int? ChangedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>An administrator deactivated a user: they lose access, their history is kept.</summary>
public sealed record UserDeactivatedEvent(int UserId, string Email, int? HotelId, int? DeactivatedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);

/// <summary>An administrator reactivated a user.</summary>
public sealed record UserActivatedEvent(int UserId, string Email, int? HotelId, int? ActivatedByUserId, DateTimeOffset OccurredOn)
    : DomainEvent(OccurredOn);
