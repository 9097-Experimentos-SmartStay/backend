using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     The user's sessions were ended (<c>auth.session_revoked</c>): a token issued before can no longer be used or
///     refreshed. <see cref="Reason"/> tells the client why, so it can explain it (e.g. "your permissions changed").
/// </summary>
public class SessionRevokedException(SessionRevocationReason? reason)
    : AuthenticationFailedException(IamErrorCodes.SessionRevoked, SessionRevocationReasons.DetailFor(reason))
{
    /// <summary>Stable reason code (<see cref="SessionRevocationReasons"/>).</summary>
    public string Reason { get; } = SessionRevocationReasons.CodeFor(reason);
}

/// <summary>Stable codes of <see cref="SessionRevocationReason"/>, as written in the <c>reason</c> of a 401.</summary>
public static class SessionRevocationReasons
{
    public const string RoleChanged = "role_changed";
    public const string AssignmentChanged = "assignment_changed";
    public const string PasswordChanged = "password_changed";
    public const string PasswordReset = "password_reset";
    public const string SignedOutEverywhere = "signed_out_everywhere";
    public const string Deactivated = "deactivated";
    public const string MfaReset = "mfa_reset";

    /// <summary>The account no longer exists, or the sessions ended before reasons were recorded.</summary>
    public const string Unknown = "unknown";

    public static string CodeFor(SessionRevocationReason? reason) => reason switch
    {
        SessionRevocationReason.RoleChanged => RoleChanged,
        SessionRevocationReason.AssignmentChanged => AssignmentChanged,
        SessionRevocationReason.PasswordChanged => PasswordChanged,
        SessionRevocationReason.PasswordReset => PasswordReset,
        SessionRevocationReason.SignedOutEverywhere => SignedOutEverywhere,
        SessionRevocationReason.Deactivated => Deactivated,
        SessionRevocationReason.MfaReset => MfaReset,
        _ => Unknown
    };

    public static string DetailFor(SessionRevocationReason? reason) => reason switch
    {
        SessionRevocationReason.RoleChanged or SessionRevocationReason.AssignmentChanged =>
            "Your permissions changed and your session was closed. Sign in again.",
        SessionRevocationReason.Deactivated => "The account has been deactivated. Contact the administrator.",
        _ => "Your session was closed. Sign in again."
    };
}
