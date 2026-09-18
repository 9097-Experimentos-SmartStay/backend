namespace BackendAwSmartstay.API.IAM.Domain.Model.Queries;

/// <summary>
///     The signed-in user, read fresh from the account (US-03 scenario 2: a role or hotel changed by an administrator
///     is visible to the user without signing in again).
/// </summary>
/// <param name="UserId">The user identified by the access token.</param>
public record GetCurrentUserQuery(int UserId);
