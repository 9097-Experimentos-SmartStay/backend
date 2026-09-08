using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert an AssignRoleResource into an AssignRoleCommand.
/// </summary>
public static class AssignRoleCommandFromResourceAssembler
{
    /// <summary>
    /// Converts the resource and actor ID to a domain command.
    /// </summary>
    /// <param name="resource">The assign role resource.</param>
    /// <param name="actorUserId">The ID of the user executing the action.</param>
    /// <returns>The command for role assignment.</returns>
    public static AssignRoleCommand ToCommandFromResource(AssignRoleResource resource, int actorUserId)
    {
        return new AssignRoleCommand(
            actorUserId,
            resource.TargetUserId,
            resource.NewRole);
    }
}