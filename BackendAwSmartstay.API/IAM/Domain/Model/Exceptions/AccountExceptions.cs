using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     US-02 scenario 3: the account is temporarily locked after too many consecutive failed sign-ins. The
///     message tells the user what happened; an e-mail notification is sent when the lock starts.
/// </summary>
public class AccountTemporarilyLockedException(DateTimeOffset lockedUntil)
    : AuthenticationFailedException(IamErrorCodes.AccountLocked,
        "The account is temporarily locked after too many failed sign-in attempts. Try again later or reset your password.")
{
    public DateTimeOffset LockedUntil { get; } = lockedUntil;
}

/// <summary>The refresh token is unknown, expired, revoked or was already used (reuse revokes its whole session).</summary>
public class InvalidRefreshTokenException()
    : AuthenticationFailedException(IamErrorCodes.RefreshTokenInvalid, "The refresh token is invalid or has expired. Sign in again.");

/// <summary>An e-mail verification or password reset link that does not exist or was already used.</summary>
public class InvalidAccountTokenException(AccountTokenPurpose purpose)
    : DomainValidationException(
        purpose == AccountTokenPurpose.PasswordReset
            ? IamErrorCodes.PasswordResetLinkInvalid
            : IamErrorCodes.EmailVerificationLinkInvalid,
        purpose == AccountTokenPurpose.PasswordReset
        ? "The password reset link is invalid or has already been used. Request a new one."
        : "The verification link is invalid or has already been used. Request a new one.");

/// <summary>US-04 scenario 3: the link is older than its lifetime; the user must request a new one.</summary>
public class AccountTokenExpiredException(AccountTokenPurpose purpose)
    : ResourceExpiredException(
        purpose == AccountTokenPurpose.PasswordReset
            ? IamErrorCodes.PasswordResetLinkExpired
            : IamErrorCodes.EmailVerificationLinkExpired,
        purpose == AccountTokenPurpose.PasswordReset
        ? "The password reset link has expired. Request a new one."
        : "The verification link has expired. Request a new one.");

/// <summary>
///     US-01: the account cannot sign in until its e-mail is verified. The user can ask for a new verification link
///     (<c>POST /authentication/verify-email/resend</c>).
/// </summary>
public class EmailNotVerifiedException()
    : OperationNotAllowedException(IamErrorCodes.EmailNotVerified, "Confirm your e-mail before signing in. Open the link we sent you, or request a new one.");

/// <summary>US-52 scenario 2: the second-factor code is wrong. It counts toward the temporary lock.</summary>
public class InvalidMfaCodeException()
    : AuthenticationFailedException(IamErrorCodes.MfaInvalidCode, "The verification code is not valid. Check your authenticator app and try again.");

/// <summary>The code was already used to sign in (replay protection): wait for the next one.</summary>
public class MfaCodeAlreadyUsedException()
    : AuthenticationFailedException(IamErrorCodes.MfaCodeAlreadyUsed, "This code was already used. Wait for the next code of your authenticator app.");

/// <summary>US-52 scenario 3: the recovery code is unknown or was already used.</summary>
public class InvalidRecoveryCodeException()
    : AuthenticationFailedException(IamErrorCodes.MfaRecoveryCodeInvalid, "The recovery code is not valid or was already used.");
