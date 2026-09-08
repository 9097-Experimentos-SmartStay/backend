namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to activate a previously deactivated user account (reverse soft delete).
/// </summary>
/// <param name="ActorUserId">The ID of the user executing this command (from JWT).</param>
/// <param name="TargetUserId">The ID of the user to activate.</param>
public record ActivateUserCommand(int ActorUserId, int TargetUserId);
