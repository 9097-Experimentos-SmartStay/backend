using System.Text.Json.Serialization;

namespace BackendAwSmartstay.API.Audit.Interfaces.REST.Resources;

/// <summary>One entry of the access audit log.</summary>
/// <param name="Id">Entry id.</param>
/// <param name="OccurredAt">Date and time (UTC, ISO 8601).</param>
/// <param name="Action">
///     Stable action code: SignInSucceeded, SignInFailed, AccountLocked, SignedOut, PasswordReset, PasswordChanged,
///     UserCreated, RoleChanged, UserDeactivated, UserActivated, MfaEnabled, MfaVerified, MfaFailed,
///     MfaRecoveryCodeUsed, MfaReset or SignedOutEverywhere.
/// </param>
/// <param name="Outcome">Success or Failure.</param>
/// <param name="ActorUserId">Who acted (null for an attempt with an unknown e-mail).</param>
/// <param name="ActorEmail">E-mail of who acted.</param>
/// <param name="TargetUserId">The account the action was about.</param>
/// <param name="TargetEmail">E-mail of that account.</param>
/// <param name="HotelId">Hotel of that account.</param>
/// <param name="IpAddress">Client IP address.</param>
/// <param name="Details">Structured facts of the action (null when there are none), e.g. <c>{ "reason": "WrongPassword" }</c> or <c>{ "previousRole": "reception", "newRole": "housekeeping" }</c>.</param>
public record AuditEntryResource(
    long Id,
    DateTimeOffset OccurredAt,
    string Action,
    string Outcome,
    int? ActorUserId,
    string? ActorEmail,
    int? TargetUserId,
    string? TargetEmail,
    int? HotelId,
    string? IpAddress,
    AuditDetailsResource? Details);

/// <summary>Structured facts of an audit entry: only the facts that apply are present; values are stable codes.</summary>
/// <param name="Reason">WrongPassword, UnknownEmail, AccountLocked, AccountDeactivated, EmailNotVerified (sign-in); InvalidCode, CodeAlreadyUsed, InvalidRecoveryCode (second factor).</param>
/// <param name="Method">AuthenticatorCode or RecoveryCode.</param>
/// <param name="Role">Role of a created user.</param>
/// <param name="PreviousRole">Role before a role change.</param>
/// <param name="NewRole">Role after a role change.</param>
/// <param name="LockedUntil">End of a temporary lock (UTC).</param>
/// <param name="RemainingRecoveryCodes">Recovery codes left.</param>
public record AuditDetailsResource(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Reason,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Method,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Role,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PreviousRole,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? NewRole,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? LockedUntil,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? RemainingRecoveryCodes);

/// <summary>A page of results.</summary>
/// <param name="Items">The items of this page.</param>
/// <param name="Page">Page number (1-based).</param>
/// <param name="PageSize">Maximum items per page.</param>
/// <param name="TotalCount">Items matching the filters.</param>
/// <param name="TotalPages">Number of pages.</param>
public record PagedResource<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
