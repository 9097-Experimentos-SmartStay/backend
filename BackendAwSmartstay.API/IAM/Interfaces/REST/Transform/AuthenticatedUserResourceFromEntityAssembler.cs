using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembler to convert a User Entity into an AuthenticatedUserResource.
/// </summary>
public static class AuthenticatedUserResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity and token to a response resource.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <param name="token">The generated token.</param>
    /// <returns>The authenticated resource with user and token information.</returns>
    public static AuthenticatedUserResource ToResourceFromEntity(User user, string token)
    {
        return new AuthenticatedUserResource(
            user.Id, 
            user.Username.Value, 
            token,
            user.Role.Value,
            user.HotelId,
            user.ChainId);
    }
}