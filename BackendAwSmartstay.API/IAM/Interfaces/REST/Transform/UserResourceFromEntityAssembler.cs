using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
/// Assembler to create a UserResource from a User entity.
/// </summary>
public static class UserResourceFromEntityAssembler
{
    /// <summary>
    /// Converts an enriched User entity to a UserResource.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>The user resource.</returns>
    public static UserResource ToResourceFromEntity(User user)
    {
        return new UserResource(
            user.Id,
            user.Username.Value,
            user.Role.Value,
            user.Status.ToString(),
            user.HotelId,
            user.ChainId,
            user.CreatedAt,
            user.UpdatedAt);
    }
}