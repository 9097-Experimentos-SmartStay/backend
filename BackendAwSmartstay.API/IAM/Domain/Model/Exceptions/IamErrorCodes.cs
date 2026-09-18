namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     Stable error codes of the IAM context: sign-in and sessions (<c>auth.*</c>), e-mail links, two-factor
///     authentication (<c>mfa.*</c>), the password policy (<c>password.*</c>) and user management (<c>user.*</c>).
/// </summary>
public static class IamErrorCodes
{
    // Sign-in and sessions
    public const string InvalidCredentials = "auth.invalid_credentials";
    public const string AccountLocked = "auth.account_locked";
    public const string AccountDeactivated = "auth.account_deactivated";
    public const string EmailNotVerified = "auth.email_not_verified";
    public const string RefreshTokenInvalid = "auth.refresh_token_invalid";

    // Bearer token of a request (written by the JWT bearer events)
    public const string TokenMissing = "auth.token_missing";
    public const string TokenInvalid = "auth.token_invalid";
    public const string TokenExpired = "auth.token_expired";
    public const string SessionRevoked = "auth.session_revoked";
    public const string Forbidden = "auth.forbidden";

    // E-mail links
    public const string EmailVerificationLinkInvalid = "email_verification.link_invalid";
    public const string EmailVerificationLinkExpired = "email_verification.link_expired";
    public const string PasswordResetLinkInvalid = "password_reset.link_invalid";
    public const string PasswordResetLinkExpired = "password_reset.link_expired";

    // Two-factor authentication (US-52)
    public const string MfaInvalidCode = "mfa.invalid_code";
    public const string MfaCodeAlreadyUsed = "mfa.code_already_used";
    public const string MfaRecoveryCodeInvalid = "mfa.recovery_code_invalid";
    public const string MfaAlreadyEnabled = "mfa.already_enabled";
    public const string MfaEnrollmentNotStarted = "mfa.enrollment_not_started";
    public const string MfaNotEnabled = "mfa.not_enabled";
    public const string MfaSecretInvalid = "mfa.secret_invalid";
    public const string MfaCodeOrRecoveryCodeRequired = "mfa.code_or_recovery_code_required";
    public const string MfaRecoveryCodeFormat = "mfa.recovery_code_format";

    // Password policy (NIST SP 800-63B-4): field violations of password / newPassword
    public const string PasswordRequired = "password.required";
    public const string PasswordTooShort = "password.too_short";
    public const string PasswordTooLong = "password.too_long";
    public const string PasswordTooCommon = "password.too_common";
    public const string PasswordRepetitive = "password.repetitive";
    public const string PasswordContainsEmail = "password.contains_email";
    public const string PasswordBreached = "password.breached";

    // Account data
    public const string NameRequired = "name.required";
    public const string NameLength = "name.length";
    public const string NameFormat = "name.invalid_format";
    public const string EmailRequired = "email.required";
    public const string EmailTooLong = "email.too_long";
    public const string EmailInvalid = "email.invalid";

    // User management (US-03)
    public const string EmailAlreadyRegistered = "user.email_already_registered";
    public const string RoleRequired = "user.role_required";
    public const string RoleUnknown = "user.role_unknown";
    public const string RoleNotAssignable = "user.role_not_assignable";
    public const string HotelOutOfScope = "user.hotel_out_of_scope";
    public const string ChainOutOfScope = "user.chain_out_of_scope";
    public const string HotelRequired = "user.hotel_required";
    public const string AdminWithoutHotel = "user.admin_without_hotel";
    public const string OutsideHierarchy = "user.outside_hierarchy";
    public const string ActorInactive = "user.actor_inactive";
    public const string LastChainAdmin = "user.last_chain_admin";
    public const string NotHotelAdministrator = "user.not_hotel_admin";
    public const string AdminAlreadyHasHotel = "hotel.admin_already_has_hotel";

    /// <summary>An invariant of an IAM aggregate that only a programming error can break.</summary>
    public const string InternalInvariant = "iam.internal_invariant";
}
