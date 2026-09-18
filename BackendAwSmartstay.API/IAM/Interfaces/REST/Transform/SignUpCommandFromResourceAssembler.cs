using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembler to convert a SignUpResource into a SignUpCommand.
/// </summary>
public static class SignUpCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the resource and optional actor ID to a domain command.
    /// </summary>
    /// <param name="resource">The sign-up resource.</param>
    /// <param name="actorUserId">The optional ID of the user executing the action.</param>
    /// <returns>The command for user registration.</returns>
    public static SignUpCommand ToCommandFromResource(SignUpResource resource, int? actorUserId = null)
    {
        return new SignUpCommand(
            resource.FirstName ?? string.Empty,
            resource.LastName ?? string.Empty,
            resource.LoginEmail,
            resource.Password ?? string.Empty,
            resource.Role, 
            actorUserId);
    }
}