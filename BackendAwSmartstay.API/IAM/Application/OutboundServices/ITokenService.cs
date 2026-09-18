using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.IAM.Application.OutboundServices;

/// <summary>A signed access token and the moment it stops being accepted.</summary>
public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
///     Issues access tokens. Validation is not part of this port: incoming tokens are validated by the
///     ASP.NET Core JWT bearer authentication handler configured in the IAM infrastructure.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a short-lived signed access token for the user.</summary>
    IssuedAccessToken GenerateToken(User user);
}
