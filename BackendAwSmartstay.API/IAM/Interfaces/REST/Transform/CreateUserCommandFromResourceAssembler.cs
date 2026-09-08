using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert a CreateUserResource into a CreateUserCommand.
/// </summary>
public static class CreateUserCommandFromResourceAssembler
{
    /// <summary>
    /// Converts the resource and actor ID to a domain command.
    /// </summary>
    /// <param name="resource">The create user resource.</param>
    /// <param name="actorUserId">The ID of the user executing the action.</param>
    /// <returns>The command for user creation.</returns>
    public static CreateUserCommand ToCommandFromResource(CreateUserResource resource, int actorUserId)
    {
        return new CreateUserCommand(
            actorUserId,
            resource.Username,
            resource.Password,
            resource.Role,
            resource.HotelId,
            resource.ChainId);
    }
}