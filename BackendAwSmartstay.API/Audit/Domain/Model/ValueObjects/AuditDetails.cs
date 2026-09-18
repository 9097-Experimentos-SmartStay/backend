namespace BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;

/// <summary>
///     Structured facts of an audit entry (never an English sentence), so each client words them in its own
///     language. Every fact is optional; values are stable codes: the IAM enum names for <see cref="Reason"/> and
///     <see cref="Method"/> and the role strings for the roles.
/// </summary>
/// <param name="Reason">
///     Why a sign-in or a second factor failed: <c>WrongPassword</c>, <c>UnknownEmail</c>, <c>AccountLocked</c>,
///     <c>AccountDeactivated</c>, <c>EmailNotVerified</c>, <c>InvalidCode</c>, <c>CodeAlreadyUsed</c> or
///     <c>InvalidRecoveryCode</c>.
/// </param>
/// <param name="Method">Second factor used: <c>AuthenticatorCode</c> or <c>RecoveryCode</c>.</param>
/// <param name="Role">Role of a created user.</param>
/// <param name="PreviousRole">Role before a role change.</param>
/// <param name="NewRole">Role after a role change.</param>
/// <param name="LockedUntil">End of a temporary lock (UTC).</param>
/// <param name="RemainingRecoveryCodes">Recovery codes left after one was used.</param>
public sealed record AuditDetails(
    string? Reason = null,
    string? Method = null,
    string? Role = null,
    string? PreviousRole = null,
    string? NewRole = null,
    DateTimeOffset? LockedUntil = null,
    int? RemainingRecoveryCodes = null)
{
    public static AuditDetails FailureReason(string reason) => new(Reason: reason);

    public static AuditDetails SecondFactor(string method, string? failureReason = null) =>
        new(Reason: failureReason, Method: method);

    public static AuditDetails Lock(DateTimeOffset lockedUntil) => new(LockedUntil: lockedUntil.ToUniversalTime());

    public static AuditDetails RecoveryCodesLeft(int remaining) => new(RemainingRecoveryCodes: remaining);

    public static AuditDetails AssignedRole(string role) => new(Role: role);

    public static AuditDetails RoleChange(string previousRole, string newRole) =>
        new(PreviousRole: previousRole, NewRole: newRole);
}
