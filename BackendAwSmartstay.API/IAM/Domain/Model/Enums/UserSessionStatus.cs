namespace BackendAwSmartstay.API.IAM.Domain.Model.Enums;

/// <summary>
///     Outcome of checking whether an access token issued to a user still represents a valid session.
/// </summary>
public enum UserSessionStatus
{
    /// <summary>The user is active and the token belongs to the current session generation.</summary>
    Valid,
    /// <summary>The user no longer exists.</summary>
    UserNotFound,
    /// <summary>The user was deactivated (US-03: a deactivated user loses access).</summary>
    Inactive,
    /// <summary>The token was issued before the last password change or deactivation.</summary>
    Revoked
}
