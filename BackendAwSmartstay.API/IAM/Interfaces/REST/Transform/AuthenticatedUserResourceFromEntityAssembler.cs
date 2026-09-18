using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Builds the sign-in/refresh response from an <see cref="AuthenticationResult"/>.
/// </summary>
public static class AuthenticatedUserResourceFromEntityAssembler
{
    public static AuthenticatedUserResource ToResourceFromResult(AuthenticationResult result)
    {
        var user = result.User;
        return new AuthenticatedUserResource
        {
            Id = user.Id,
            Username = user.Email.Value,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.Value,
            HotelId = user.HotelId,
            ChainId = user.ChainId,
            EmailVerified = user.EmailVerified,
            Token = result.AccessToken,
            ExpiresAt = result.AccessTokenExpiresAt,
            RefreshToken = result.RefreshToken?.Value,
            RefreshTokenExpiresAt = result.RefreshToken?.ExpiresAt
        };
    }
}
