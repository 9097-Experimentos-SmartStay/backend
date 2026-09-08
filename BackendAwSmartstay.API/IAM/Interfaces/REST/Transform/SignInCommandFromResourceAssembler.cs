using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
/// Assembler to create a SignInCommand from a SignInResource.
/// </summary>
public static class SignInCommandFromResourceAssembler
{
    /// <summary>
    /// Converts a SignInResource to a SignInCommand.
    /// </summary>
    /// <param name="resource">The sign-in resource.</param>
    /// <returns>The sign-in command.</returns>
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        return new SignInCommand(resource.Username, resource.Password);
    }
}