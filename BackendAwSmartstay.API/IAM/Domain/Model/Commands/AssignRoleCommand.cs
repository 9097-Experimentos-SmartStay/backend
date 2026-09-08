namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to assign a new role to an existing user.
/// </summary>
/// <param name="ActorUserId">The ID of the user executing this command (from JWT).</param>
/// <param name="TargetUserId">The ID of the user whose role will change.</param>
/// <param name="NewRole">The role to assign.</param>
public record AssignRoleCommand(int ActorUserId, int TargetUserId, string NewRole);