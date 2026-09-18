using BackendAwSmartstay.API.IAM.Application.Internal.Configuration;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Events;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Orchestrates the account and session use cases. Rules live in the aggregates (<see cref="User"/> lockout and
///     verification, <see cref="AccountToken"/> single use and expiry, <see cref="RefreshToken"/> rotation); this
///     service loads them, commits, and only then sends e-mails.
/// </summary>
public class AuthenticationCommandService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    AccountTokenIssuer accountTokenIssuer,
    ITokenService tokenService,
    IHashingService hashingService,
    ISecureTokenGenerator secureTokenGenerator,
    IRoleAuthorizationService roleAuthorizationService,
    IAccountNotificationService notifications,
    IDomainEventDispatcher domainEventDispatcher,
    IUnitOfWork unitOfWork,
    NewPasswordValidator newPasswordValidator,
    SessionIssuer sessionIssuer,
    IOptions<AccountSecuritySettings> settings,
    TimeProvider timeProvider,
    ILogger<AuthenticationCommandService> logger) : IAuthenticationCommandService
{
    // Verifying against a real bcrypt hash when the e-mail is unknown keeps the response time of an unknown
    // e-mail close to that of a wrong password (no account enumeration by timing, US-02 scenario 2).
    private static string? _unknownAccountHash;

    private AccountSecuritySettings Settings => settings.Value;

    // ── Sign-in (US-02) ─────────────────────────────────────────────────────

    public async Task<AuthenticationResult> Handle(SignInCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var email = new Email(command.Email);
        var user = await userRepository.FindByEmailAsync(email);

        if (user is null)
        {
            hashingService.VerifyPassword(command.Password, _unknownAccountHash ??= hashingService.HashPassword(Guid.NewGuid().ToString()));
            await domainEventDispatcher.DispatchAsync(
                [new SignInFailedEvent(null, email.Value, null, SignInFailureReason.UnknownEmail, now)]);
            throw new InvalidCredentialsException();
        }

        if (user.IsLockedOut(now))
        {
            user.RejectSignInWhileLocked(now);
            await unitOfWork.CompleteAsync();
            throw new AccountTemporarilyLockedException(user.LockedUntil!.Value);
        }

        if (!hashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            var lockStarted = user.RegisterFailedSignIn(Settings.LockoutPolicy, now);
            await unitOfWork.CompleteAsync();
            if (!lockStarted) throw new InvalidCredentialsException();

            await notifications.SendAccountLockedAsync(user, user.LockedUntil!.Value);
            throw new AccountTemporarilyLockedException(user.LockedUntil!.Value);
        }

        if (user.Status == UserStatus.Inactive)
        {
            user.RejectSignInWhileDeactivated(now);
            await unitOfWork.CompleteAsync();
            throw new UnauthorizedOperationException(IamErrorCodes.AccountDeactivated, "The account has been deactivated. Contact the administrator.");
        }

        if (!user.EmailVerified)
        {
            user.RejectSignInWithUnverifiedEmail(now);
            await unitOfWork.CompleteAsync();
            throw new EmailNotVerifiedException();
        }

        // US-52: the password is only the first factor of a staff account (and of any account with MFA enabled).
        // Nothing is granted yet and the failure counter is not reset: only the second factor completes the sign-in.
        if (user.MfaEnabled || user.RequiresMfaEnrollment)
        {
            var kind = user.MfaEnabled ? MfaChallengeKind.Verification : MfaChallengeKind.Enrollment;
            return AuthenticationResult.SecondFactorPending(user, tokenService.GenerateMfaChallengeToken(user, kind, command.RememberMe));
        }

        return await sessionIssuer.StartSessionAsync(user, command.RememberMe, now);
    }

    // ── Remembered sessions (US-02 scenario 4) ──────────────────────────────

    public async Task<AuthenticationResult> Handle(RefreshSessionCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var presented = await refreshTokenRepository.FindByHashAsync(secureTokenGenerator.Hash(command.RefreshToken))
                        ?? throw new InvalidRefreshTokenException();

        if (presented.WasRotated)
        {
            // Replay of a token that was already exchanged: somebody else holds the session. Revoke it all.
            logger.LogWarning("Refresh token reuse detected for user {UserId}: the session is revoked.", presented.UserId);
            await RevokeAsync(await refreshTokenRepository.ListUnrevokedByFamilyAsync(presented.FamilyId),
                RefreshTokenRevocationReason.ReuseDetected, now);
            throw new InvalidRefreshTokenException();
        }

        var user = await userRepository.FindByIdAsync(presented.UserId);

        // The user's sessions ended after this token was issued (role or hotel change, password, deactivation,
        // sign-out everywhere...): say why, whether or not the token row was already revoked.
        if (user is not null && !presented.BelongsToCurrentSessionOf(user))
        {
            if (presented.IsActive(now))
            {
                await RevokeAsync([presented], RefreshTokenRevocationReason.SessionRevoked, now);
            }
            throw new SessionRevokedException(user.GetSession(presented.TokenVersion).RevocationReason);
        }

        if (!presented.IsActive(now)) throw new InvalidRefreshTokenException();

        if (user is null || user.Status == UserStatus.Inactive)
        {
            await RevokeAsync([presented], RefreshTokenRevocationReason.SessionRevoked, now);
            throw new SessionRevokedException(user?.GetSession(presented.TokenVersion).RevocationReason);
        }
        // A staff account without a second factor (e.g. after an MFA reset) must sign in again and enroll (US-52).
        if (user.RequiresMfaEnrollment)
        {
            await RevokeAsync([presented], RefreshTokenRevocationReason.SessionRevoked, now);
            throw new InvalidRefreshTokenException();
        }

        var successorToken = secureTokenGenerator.Generate();
        var successor = presented.Rotate(successorToken.Hash, now, Settings.RefreshTokenLifetime);
        await refreshTokenRepository.AddAsync(successor);
        await unitOfWork.CompleteAsync();

        var accessToken = tokenService.GenerateToken(user, successor.FamilyId);
        return new AuthenticationResult(user, accessToken.Value, accessToken.ExpiresAt,
            new IssuedRefreshToken(successorToken.Value, successor.ExpiresAt));
    }

    public async Task Handle(SignOutCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var presented = await refreshTokenRepository.FindByHashAsync(secureTokenGenerator.Hash(command.RefreshToken));
        if (presented is null) return; // Signing out twice (or with an unknown token) is not an error.

        foreach (var token in await refreshTokenRepository.ListUnrevokedByFamilyAsync(presented.FamilyId))
            token.Revoke(RefreshTokenRevocationReason.SignedOut, now);

        var user = await userRepository.FindByIdAsync(presented.UserId);
        user?.SignOut(now);
        await unitOfWork.CompleteAsync();
    }

    // ── Registration and e-mail verification (US-01) ────────────────────────

    public async Task<User> Handle(SignUpCommand command)
    {
        var email = new Email(command.Email);
        var name = new PersonName(command.FirstName, command.LastName);
        if (await userRepository.ExistsByEmailAsync(email))
            throw new EmailAlreadyRegisteredException(email.Value);

        // Validate the requested role first so an unknown role is a 400, not a 403.
        var requestedRole = string.IsNullOrWhiteSpace(command.Role) ? null : new Role(command.Role.Trim().ToLowerInvariant());
        var role = new Role(UserRoles.Guest);
        if (requestedRole is not null && requestedRole.Value != UserRoles.Guest)
        {
            if (command.ActorUserId is null)
                throw new UnauthorizedOperationException(IamErrorCodes.RoleNotAssignable, "Authentication required to assign a specific role during sign-up.");

            var actor = await userRepository.FindByIdAsync(command.ActorUserId.Value);
            if (actor is null || actor.Status == UserStatus.Inactive || !roleAuthorizationService.CanAssignRole(actor, requestedRole.Value))
                throw new UnauthorizedOperationException(IamErrorCodes.RoleNotAssignable, $"You cannot assign the role '{requestedRole.Value}'.");
            role = requestedRole;
        }

        await newPasswordValidator.EnsureAcceptableAsync(command.Password, role, email, nameof(command.Password));

        var user = User.Register(name, email, hashingService.HashPassword(command.Password), role,
            hotelId: null, chainId: null, createdByUserId: command.ActorUserId, timeProvider.GetUtcNow());

        PendingAccountToken? verification = null;
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
            verification = await accountTokenIssuer.IssueAsync(user, AccountTokenPurpose.EmailVerification);
            await unitOfWork.CompleteAsync();
        });

        await notifications.SendEmailVerificationAsync(user, verification!.Value, verification.ExpiresAt);
        return user;
    }

    public async Task Handle(VerifyEmailCommand command)
    {
        var token = await accountTokenIssuer.ConsumeAsync(command.Token, AccountTokenPurpose.EmailVerification);
        var user = await userRepository.FindByIdAsync(token.UserId)
                   ?? throw new InvalidAccountTokenException(AccountTokenPurpose.EmailVerification);
        user.VerifyEmail(timeProvider.GetUtcNow());
        await unitOfWork.CompleteAsync();
    }

    public async Task Handle(ResendEmailVerificationCommand command)
    {
        var user = await userRepository.FindByEmailAsync(new Email(command.Email));
        if (user is null || user.EmailVerified || user.Status == UserStatus.Inactive) return;

        var verification = await accountTokenIssuer.IssueAsync(user, AccountTokenPurpose.EmailVerification);
        await unitOfWork.CompleteAsync();
        await notifications.SendEmailVerificationAsync(user, verification.Value, verification.ExpiresAt);
    }

    // ── Password recovery (US-04) ───────────────────────────────────────────

    public async Task Handle(RequestPasswordRecoveryCommand command)
    {
        var user = await userRepository.FindByEmailAsync(new Email(command.Email));
        if (user is null || user.Status == UserStatus.Inactive)
        {
            logger.LogInformation("Password recovery requested for an e-mail without an active account.");
            return;
        }

        var reset = await accountTokenIssuer.IssueAsync(user, AccountTokenPurpose.PasswordReset);
        await unitOfWork.CompleteAsync();
        await notifications.SendPasswordResetLinkAsync(user, reset.Value, reset.ExpiresAt);
    }

    public async Task Handle(ResetPasswordCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var token = await accountTokenIssuer.ConsumeAsync(command.Token, AccountTokenPurpose.PasswordReset);
        var user = await userRepository.FindByIdAsync(token.UserId)
                   ?? throw new InvalidAccountTokenException(AccountTokenPurpose.PasswordReset);
        // Nothing is committed when the new password is rejected: the link can be used again with another one.
        await newPasswordValidator.EnsureAcceptableAsync(command.NewPassword, user.Role, user.Email, nameof(command.NewPassword));

        user.ResetPassword(hashingService.HashPassword(command.NewPassword), now);
        foreach (var session in await refreshTokenRepository.ListUnrevokedByUserAsync(user.Id))
            session.Revoke(RefreshTokenRevocationReason.SessionRevoked, now);

        await unitOfWork.CompleteAsync();
        await notifications.SendPasswordChangedAsync(user);
    }

    private async Task RevokeAsync(IEnumerable<RefreshToken> tokens, RefreshTokenRevocationReason reason, DateTimeOffset now)
    {
        foreach (var token in tokens) token.Revoke(reason, now);
        await unitOfWork.CompleteAsync();
    }
}
