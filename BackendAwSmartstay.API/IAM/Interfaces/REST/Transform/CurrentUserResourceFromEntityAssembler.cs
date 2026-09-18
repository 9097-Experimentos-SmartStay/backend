using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

public static class CurrentUserResourceFromEntityAssembler
{
    public static CurrentUserResource ToResourceFromEntity(User user) => new(
        user.Id,
        user.Email.Value,
        user.FirstName,
        user.LastName,
        user.Role.Value,
        user.HotelId,
        user.ChainId,
        user.EmailVerified,
        user.MfaEnabled);
}
