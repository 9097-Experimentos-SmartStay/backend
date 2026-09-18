using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>An issued refresh token: <see cref="Value"/> is returned once to the client.</summary>
public sealed record IssuedRefreshToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>Result of a successful sign-in or refresh.</summary>
/// <param name="User">The authenticated user.</param>
/// <param name="AccessToken">Short-lived bearer token.</param>
/// <param name="AccessTokenExpiresAt">When the access token expires.</param>
/// <param name="RefreshToken">Only for remembered sessions.</param>
public sealed record AuthenticationResult(User User, string AccessToken, DateTimeOffset AccessTokenExpiresAt, IssuedRefreshToken? RefreshToken);

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
