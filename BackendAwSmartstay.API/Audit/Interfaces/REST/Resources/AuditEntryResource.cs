namespace BackendAwSmartstay.API.Audit.Interfaces.REST.Resources;

/// <summary>One entry of the access audit log.</summary>
/// <param name="Id">Entry id.</param>
/// <param name="OccurredAt">Date and time (UTC, ISO 8601).</param>
/// <param name="Action">SignInSucceeded, SignInFailed, AccountLocked, SignedOut, PasswordReset, PasswordChanged, UserCreated, RoleChanged, UserDeactivated or UserActivated.</param>
/// <param name="Outcome">Success or Failure.</param>
/// <param name="ActorUserId">Who acted (null for an attempt with an unknown e-mail).</param>
/// <param name="ActorEmail">E-mail of who acted.</param>
/// <param name="TargetUserId">The account the action was about.</param>
/// <param name="TargetEmail">E-mail of that account.</param>
/// <param name="HotelId">Hotel of that account.</param>
/// <param name="IpAddress">Client IP address.</param>
/// <param name="Details">Extra facts, e.g. "Reason: WrongPassword" or "Role: reception -> housekeeping".</param>
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
    string? Details);

/// <summary>A page of results.</summary>
/// <param name="Items">The items of this page.</param>
/// <param name="Page">Page number (1-based).</param>
/// <param name="PageSize">Maximum items per page.</param>
/// <param name="TotalCount">Items matching the filters.</param>
/// <param name="TotalPages">Number of pages.</param>
public record PagedResource<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
