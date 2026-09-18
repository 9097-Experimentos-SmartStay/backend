namespace BackendAwSmartstay.API.IAM.Domain.Model.Queries;

/// <summary>The signed-in user's account (profile, like OpenID Connect <c>userinfo</c>).</summary>
/// <param name="UserId">The user identified by the access token.</param>
public record GetCurrentUserQuery(int UserId);
