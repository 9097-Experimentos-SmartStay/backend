namespace BackendAwSmartstay.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to retrieve all users that a specific actor has access to, based on their organizational scope.
/// </summary>
/// <param name="ActorUserId">The ID of the user executing the query.</param>
public record GetUsersByScopeQuery(int ActorUserId);