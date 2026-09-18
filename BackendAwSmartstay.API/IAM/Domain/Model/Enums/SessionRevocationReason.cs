namespace BackendAwSmartstay.API.IAM.Domain.Model.Enums;

/// <summary>
///     Why the sessions of a user were ended (every access and refresh token issued before stops working). The
///     401 of a revoked token says it, so the client can tell the user what happened.
/// </summary>
public enum SessionRevocationReason
{
    /// <summary>An administrator changed the user's role (US-03 scenario 2): sign in again with the new permissions.</summary>
    RoleChanged,
    /// <summary>The user's hotel or chain changed (an administrator, or the admin registered their hotel).</summary>
    AssignmentChanged,
    /// <summary>The user changed their password.</summary>
    PasswordChanged,
    /// <summary>The password was reset through the recovery link.</summary>
    PasswordReset,
    /// <summary>The user signed out of every device.</summary>
    SignedOutEverywhere,
    /// <summary>An administrator deactivated the account.</summary>
    Deactivated,
    /// <summary>An administrator reset the user's two-factor authentication.</summary>
    MfaReset
}
