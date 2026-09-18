using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>An issued refresh token: <see cref="Value"/> is returned once to the client.</summary>
public sealed record IssuedRefreshToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
///     Result of a sign-in step: either a session (access token, and a refresh token for remembered sessions) or,
///     after a correct password on an account with a second factor pending, a challenge token (US-52).
/// </summary>
/// <param name="User">The user.</param>
/// <param name="AccessToken">Short-lived bearer token; null while a second factor is pending.</param>
/// <param name="AccessTokenExpiresAt">When the access token expires.</param>
/// <param name="RefreshToken">Only for remembered sessions.</param>
/// <param name="MfaChallenge">The second-factor challenge, when the password step is done but access is not granted yet.</param>
/// <param name="RecoveryCodes">The one-time recovery codes, only right after enabling MFA (shown once).</param>
public sealed record AuthenticationResult(
    User User,
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresAt,
    IssuedRefreshToken? RefreshToken,
    IssuedMfaChallengeToken? MfaChallenge = null,
    IReadOnlyList<string>? RecoveryCodes = null)
{
    public static AuthenticationResult SecondFactorPending(User user, IssuedMfaChallengeToken challenge) =>
        new(user, null, null, null, challenge);
}

/// <summary>
///     Account and session use cases of the IAM context: registration and e-mail verification (US-01), secure
///     sign-in with temporary lock and remembered sessions (US-02), password recovery (US-04).
/// </summary>
public interface IAuthenticationCommandService
{
    Task<AuthenticationResult> Handle(SignInCommand command);

    Task<AuthenticationResult> Handle(RefreshSessionCommand command);

    Task Handle(SignOutCommand command);

    Task<User> Handle(SignUpCommand command);

    Task Handle(VerifyEmailCommand command);

    Task Handle(ResendEmailVerificationCommand command);

    Task Handle(RequestPasswordRecoveryCommand command);

    Task Handle(ResetPasswordCommand command);
}
