using BackendAwSmartstay.API.IAM.Application.Internal.Configuration;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Orchestrates two-factor authentication (US-52). The rules (replay protection, lockout, enrollment state) live
///     in the <see cref="User"/> aggregate and <see cref="TotpAlgorithm"/>; this service decrypts the secrets for them,
///     stores the recovery code hashes, commits and grants the session.
/// </summary>
public class MfaCommandService(
    IUserRepository userRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRoleAuthorizationService roleAuthorizationService,
    ISecretProtector secretProtector,
    IHashingService hashingService,
    IAccountNotificationService notifications,
    SessionIssuer sessionIssuer,
    IUnitOfWork unitOfWork,
    IOptions<AccountSecuritySettings> accountSecurity,
    IOptions<MfaSettings> mfaSettings,
    TimeProvider timeProvider) : IMfaCommandService
{
    /// <summary>Data Protection purpose of the TOTP secrets.</summary>
    private const string SecretPurpose = "IAM.MfaTotpSecret";

    private SignInLockoutPolicy LockoutPolicy => accountSecurity.Value.LockoutPolicy;

    public async Task<MfaEnrollment> Handle(StartMfaEnrollmentCommand command)
    {
        var user = await FindUserAsync(command.UserId);
        var secret = TotpSecret.Generate();
        user.StartMfaEnrollment(secretProtector.Protect(SecretPurpose, secret.ToBase32()));
        await unitOfWork.CompleteAsync();

        var issuer = mfaSettings.Value.Issuer;
        var account = user.Email.Value;
        var label = Uri.EscapeDataString($"{issuer}:{account}");
        var uri = $"otpauth://totp/{label}?secret={secret.ToBase32()}&issuer={Uri.EscapeDataString(issuer)}" +
                  $"&algorithm={TotpAlgorithm.Algorithm}&digits={TotpAlgorithm.Digits}&period={TotpAlgorithm.PeriodSeconds}";
        return new MfaEnrollment(secret.ToBase32(), uri, issuer, account, TotpAlgorithm.Digits,
            TotpAlgorithm.PeriodSeconds, TotpAlgorithm.Algorithm);
    }

    public async Task<AuthenticationResult> Handle(ConfirmMfaEnrollmentCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var user = await FindUserAsync(command.UserId);
        await EnsureNotLockedAsync(user, now);

        user.EnsureMfaEnrollmentInProgress();
        var pendingSecret = TotpSecret.FromBase32(secretProtector.Unprotect(SecretPurpose, user.MfaPendingSecretProtected!));
        var outcome = user.ConfirmMfaEnrollment(pendingSecret, command.Code, LockoutPolicy, now);
        await ThrowUnlessAcceptedAsync(user, outcome, new InvalidMfaCodeException());

        // US-52 scenario 3: ten one-time recovery codes, shown once; only their hashes are stored.
        var codes = Enumerable.Range(0, RecoveryCodeFormat.CodesPerEnrollment).Select(_ => RecoveryCodeFormat.NewCode()).ToList();
        await recoveryCodeRepository.RemoveAllOfUserAsync(user.Id);
        foreach (var code in codes)
            await recoveryCodeRepository.AddAsync(MfaRecoveryCode.Issue(user.Id,
                hashingService.HashPassword(RecoveryCodeFormat.Canonicalize(code)), now));

        return await sessionIssuer.StartSessionAsync(user, command.RememberMe, now, codes);
    }

    public async Task<AuthenticationResult> Handle(VerifyMfaCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var user = await FindUserAsync(command.UserId);
        await EnsureNotLockedAsync(user, now);

        if (!string.IsNullOrWhiteSpace(command.RecoveryCode))
        {
            var canonical = RecoveryCodeFormat.Canonicalize(command.RecoveryCode);
            var unused = await recoveryCodeRepository.ListUnusedByUserAsync(user.Id);
            var match = unused.FirstOrDefault(code => hashingService.VerifyPassword(canonical, code.CodeHash));
            if (match is null)
            {
                await ThrowUnlessAcceptedAsync(user, user.RejectRecoveryCode(LockoutPolicy, now), new InvalidRecoveryCodeException());
                throw new InvalidRecoveryCodeException();
            }

            match.Redeem(now);
            user.AcceptRecoveryCode(unused.Count - 1, now);
            return await sessionIssuer.StartSessionAsync(user, command.RememberMe, now);
        }

        user.EnsureMfaEnabled();
        var secret = TotpSecret.FromBase32(secretProtector.Unprotect(SecretPurpose, user.MfaSecretProtected!));
        var outcome = user.VerifyMfaCode(secret, command.Code ?? string.Empty, LockoutPolicy, now);
        await ThrowUnlessAcceptedAsync(user, outcome,
            outcome == MfaCodeOutcome.Replayed ? new MfaCodeAlreadyUsedException() : new InvalidMfaCodeException());

        return await sessionIssuer.StartSessionAsync(user, command.RememberMe, now);
    }

    public async Task Handle(ResetMfaCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var actor = await userRepository.FindByIdAsync(command.ActorUserId)
                    ?? throw new UserNotFoundException(command.ActorUserId);
        var target = await userRepository.FindByIdAsync(command.TargetUserId)
                     ?? throw new UserNotFoundException(command.TargetUserId);
        if (actor.Status == UserStatus.Inactive || !roleAuthorizationService.CanManage(actor, target))
            throw new UnauthorizedOperationException(IamErrorCodes.OutsideHierarchy, $"You cannot reset the two-factor authentication of user {target.Id}.");

        target.ResetMfa(actor.Id, now);
        await recoveryCodeRepository.RemoveAllOfUserAsync(target.Id);
        foreach (var session in await refreshTokenRepository.ListUnrevokedByUserAsync(target.Id))
            session.Revoke(RefreshTokenRevocationReason.SessionRevoked, now);
        await unitOfWork.CompleteAsync();
    }

    public async Task Handle(SignOutEverywhereCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var user = await FindUserAsync(command.UserId);
        user.SignOutEverywhere(now);
        foreach (var session in await refreshTokenRepository.ListUnrevokedByUserAsync(user.Id))
            session.Revoke(RefreshTokenRevocationReason.SignedOut, now);
        await unitOfWork.CompleteAsync();
    }

    private async Task<User> FindUserAsync(int userId) =>
        await userRepository.FindByIdAsync(userId) ?? throw new UserNotFoundException(userId);

    /// <summary>While the account is locked no code is even checked (same as the password step).</summary>
    private async Task EnsureNotLockedAsync(User user, DateTimeOffset now)
    {
        if (!user.IsLockedOut(now)) return;
        user.RejectSignInWhileLocked(now);
        await unitOfWork.CompleteAsync();
        throw new AccountTemporarilyLockedException(user.LockedUntil!.Value);
    }

    /// <summary>Commits the counted failure (and the lock e-mail when it starts) before rejecting the code.</summary>
    private async Task ThrowUnlessAcceptedAsync(User user, MfaCodeOutcome outcome, Exception rejection)
    {
        if (outcome == MfaCodeOutcome.Accepted) return;
        if (outcome == MfaCodeOutcome.LockStarted)
            await notifications.SendAccountLockedAsync(user, user.LockedUntil!.Value);
        await unitOfWork.CompleteAsync();
        if (outcome != MfaCodeOutcome.LockStarted) throw rejection;
        throw new AccountTemporarilyLockedException(user.LockedUntil!.Value);
    }
}
