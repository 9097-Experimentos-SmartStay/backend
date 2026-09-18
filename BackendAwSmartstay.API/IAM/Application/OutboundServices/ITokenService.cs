using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.IAM.Application.OutboundServices;

/// <summary>A signed access token and the moment it stops being accepted.</summary>
public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>What a second-factor challenge token allows (US-52).</summary>
public enum MfaChallengeKind
{
    /// <summary>Only the enrollment endpoints: the staff account has no authenticator yet.</summary>
    Enrollment,
    /// <summary>Only the verification endpoint: the account must present a code.</summary>
    Verification
}

/// <summary>A short-lived token that proves the password step and only opens the second-factor endpoints.</summary>
public sealed record IssuedMfaChallengeToken(MfaChallengeKind Kind, string Value, DateTimeOffset ExpiresAt);

/// <summary>
///     Issues access tokens. Validation is not part of this port: incoming tokens are validated by the
///     ASP.NET Core JWT bearer authentication handler configured in the IAM infrastructure.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a short-lived signed access token for the user.</summary>
    IssuedAccessToken GenerateToken(User user);

    /// <summary>
    ///     Generates the limited token returned after a correct password when a second factor is pending. It is not
    ///     an access token: the API only accepts it on the matching second-factor endpoints.
    /// </summary>
    IssuedMfaChallengeToken GenerateMfaChallengeToken(User user, MfaChallengeKind kind, bool rememberMe);
}
