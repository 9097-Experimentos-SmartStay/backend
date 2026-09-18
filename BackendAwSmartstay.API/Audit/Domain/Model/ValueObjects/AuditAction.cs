namespace BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;

/// <summary>Access-related actions recorded in the audit log (US-03 scenario 4).</summary>
public enum AuditAction
{
    SignInSucceeded,
    SignInFailed,
    AccountLocked,
    SignedOut,
    PasswordReset,
    PasswordChanged,
    UserCreated,
    RoleChanged,
    UserDeactivated,
    UserActivated
}

/// <summary>Whether the recorded action succeeded.</summary>
public enum AuditOutcome
{
    Success,
    Failure
}
