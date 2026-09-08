namespace BackendAwSmartstay.API.IAM.Domain.Model.Queries;

/**
 * <summary>
 * The get user by id query
 * </summary>
 * <remarks>
 * This query object includes the user id to search, and optionally the actor's id to enforce scope.
 * </remarks>
 */
public record GetUserByIdQuery(int Id, int? ActorUserId = null);