namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to deactivate a user account (soft delete).
/// </summary>
/// <param name="ActorUserId">The ID of the user executing this command (from JWT).</param>
/// <param name="TargetUserId">The ID of the user to deactivate.</param>
public record DeactivateUserCommand(int ActorUserId, int TargetUserId);