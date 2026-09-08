using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert an UpdateUserResource into an UpdateUserCommand.
/// </summary>
public static class UpdateUserCommandFromResourceAssembler
{
    /// <summary>
    /// Converts the resource, target ID, and actor ID to a domain command.
    /// </summary>
    /// <param name="resource">The update user resource.</param>
    /// <param name="actorUserId">The ID of the user executing the action.</param>
    /// <param name="targetUserId">The ID of the user being updated.</param>
    /// <returns>The command for updating user attributes.</returns>
    public static UpdateUserCommand ToCommandFromResource(UpdateUserResource resource, int actorUserId, int targetUserId)
    {
        return new UpdateUserCommand(
            actorUserId,
            targetUserId,
            resource.NewUsername,
            resource.NewPassword,
            resource.NewHotelId,
            resource.NewChainId);
    }
}