using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.IAM.Application.OutboundServices;

/// <summary>
///     Issues access tokens. Validation is not part of this port: incoming tokens are validated by the
///     ASP.NET Core JWT bearer authentication handler configured in the IAM infrastructure.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed access token for the user.</summary>
    string GenerateToken(User user);
}
