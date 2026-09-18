using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.IAM.Application.OutboundServices;

/// <summary>
///     E-mails about the account. The application services call it before the unit of work commits the change the
///     e-mail announces (transactional outbox: stored only if the change commits).
/// </summary>
public interface IAccountNotificationService
{
    /// <summary>US-01: confirmation of the registration with the verification link.</summary>
    Task SendEmailVerificationAsync(User user, string verificationToken, DateTimeOffset expiresAt);

    /// <summary>US-02 scenario 3: the account was temporarily locked.</summary>
    Task SendAccountLockedAsync(User user, DateTimeOffset lockedUntil);

    /// <summary>US-04 scenario 1: link to choose a new password.</summary>
    Task SendPasswordResetLinkAsync(User user, string resetToken, DateTimeOffset expiresAt);

    /// <summary>US-04 scenario 4: the password was changed.</summary>
    Task SendPasswordChangedAsync(User user);

    /// <summary>
    ///     US-03 scenario 2: an administrator changed the user's role or hotel; their sessions ended and they must
    ///     sign in again.
    /// </summary>
    Task SendPermissionsChangedAsync(User user);
}
