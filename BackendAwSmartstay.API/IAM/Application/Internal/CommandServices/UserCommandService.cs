using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>
/// Service responsible for handling user-related commands (Write operations).
/// Enforces authorization and scope rules for all management operations.
/// </summary>
public class UserCommandService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    AccountTokenIssuer accountTokenIssuer,
    IAccountNotificationService notifications,
    TimeProvider timeProvider,
    IHashingService hashingService,
    IRoleAuthorizationService roleAuthorizationService,
    IUserScopeService userScopeService,
    NewPasswordValidator newPasswordValidator,
    SessionIssuer sessionIssuer,
    IUnitOfWork unitOfWork) : IUserCommandService
{
    /// <summary>
    /// Processes a password change request.
    /// </summary>
    public async Task Handle(ChangePasswordCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new UserNotFoundException(command.UserId);

        if (!hashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            throw new InvalidCredentialsException();

        await newPasswordValidator.EnsureAcceptableAsync(command.NewPassword, user.Role, user.Email, nameof(command.NewPassword));
        user.ChangePassword(hashingService.HashPassword(command.NewPassword), timeProvider.GetUtcNow());
        foreach (var session in await refreshTokenRepository.ListUnrevokedByUserAsync(user.Id))
            session.Revoke(RefreshTokenRevocationReason.SessionRevoked, timeProvider.GetUtcNow());

        await unitOfWork.CompleteAsync();
    }

    // ═══════════════════════════════════════════════════════════
    // New management commands
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new user with explicit role, hotel and chain assignment.
    /// Only actors with hierarchy superiority and scope access can create users.
    /// </summary>
    public async Task<User> Handle(CreateUserCommand command)
    {
        var actor = await ResolveActorAsync(command.ActorUserId);
        var role = new Role(command.Role.Trim().ToLowerInvariant());

        if (!roleAuthorizationService.CanAssignRole(actor, role.Value))
            throw new UnauthorizedOperationException(IamErrorCodes.RoleNotAssignable, $"You cannot assign the role '{role.Value}'.");

        var hotelId = StaffAccountPolicy.ResolveHotel(actor, role.Value, command.HotelId);
        if (hotelId.HasValue && !userScopeService.CanAccessHotel(actor, hotelId))
            throw new UnauthorizedOperationException(IamErrorCodes.HotelOutOfScope, $"You cannot create users for hotel {hotelId}.");

        if (command.ChainId.HasValue && !roleAuthorizationService.CanAssignChainId(actor, command.ChainId))
            throw new UnauthorizedOperationException(IamErrorCodes.ChainOutOfScope, $"You cannot assign chain {command.ChainId}.");

        var email = new Email(command.Email);
        if (await userRepository.ExistsByEmailAsync(email))
            throw new EmailAlreadyRegisteredException(email.Value);

        var name = new PersonName(command.FirstName, command.LastName);
        await newPasswordValidator.EnsureAcceptableAsync(command.Password, role, email, nameof(command.Password));
        var user = User.Register(name, email, hashingService.HashPassword(command.Password), role,
            hotelId, command.ChainId, createdByUserId: actor.Id, timeProvider.GetUtcNow());

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

    /// <summary>
    /// Updates an existing user's attributes.
    /// Null fields are ignored.
    /// Actor must have hierarchy superiority over the target and scope access.
    /// </summary>
    public async Task Handle(UpdateUserCommand command)
    {
        var actor = await ResolveActorAsync(command.ActorUserId);
        var target = await ResolveTargetAsync(command.TargetUserId);

        if (!roleAuthorizationService.CanManage(actor, target))
            throw new UnauthorizedOperationException(IamErrorCodes.OutsideHierarchy,
                $"User {actor.Id} cannot manage user {target.Id}.");

        if (command.NewEmail is not null)
        {
            var newEmail = new Email(command.NewEmail);
            if (newEmail != target.Email && await userRepository.ExistsByEmailAsync(newEmail))
                throw new EmailAlreadyRegisteredException(newEmail.Value);

            target.UpdateEmail(newEmail.Value);
        }

        if (command.NewPassword is not null)
        {
            await newPasswordValidator.EnsureAcceptableAsync(command.NewPassword, target.Role, target.Email, nameof(command.NewPassword));
            var hashed = hashingService.HashPassword(command.NewPassword);
            target.UpdatePasswordHash(hashed);
        }

        if (command.NewHotelId.HasValue && !userScopeService.CanAccessHotel(actor, command.NewHotelId))
            throw new UnauthorizedOperationException(IamErrorCodes.HotelOutOfScope,
                $"User {actor.Id} cannot assign hotel {command.NewHotelId}.");

        if (command.NewChainId.HasValue && !roleAuthorizationService.CanAssignChainId(actor, command.NewChainId))
            throw new UnauthorizedOperationException(IamErrorCodes.ChainOutOfScope,
                $"User {actor.Id} cannot assign chain {command.NewChainId}.");

        // A new hotel or chain ends the user's sessions: their tokens carry the old scope.
        var assignmentChanged = target.ChangeAssignment(command.NewHotelId ?? target.HotelId,
            command.NewChainId ?? target.ChainId, actor.Id, timeProvider.GetUtcNow());

        if (assignmentChanged)
            await EndSessionsAsync(target, notifyUser: true);
        else
            await unitOfWork.CompleteAsync();
    }

    /// <summary>
    /// Assigns a new role to an existing user.
    /// Actor must be allowed to assign the target role and must manage the target user.
    /// </summary>
    public async Task Handle(AssignRoleCommand command)
    {
        var actor = await ResolveActorAsync(command.ActorUserId);
        var target = await ResolveTargetAsync(command.TargetUserId);

        if (!roleAuthorizationService.CanManage(actor, target))
            throw new UnauthorizedOperationException(IamErrorCodes.OutsideHierarchy,
                $"User {actor.Id} cannot manage user {target.Id}.");

        if (!roleAuthorizationService.CanAssignRole(actor, command.NewRole))
            throw new UnauthorizedOperationException(IamErrorCodes.RoleNotAssignable,
                $"User {actor.Id} cannot assign role '{command.NewRole}'.");

        // --- NEW RULE: Protect the last ChainAdmin ---
        if (string.Equals(target.Role.Value, UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(command.NewRole, UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase) &&
            target.Status == UserStatus.Active)
        {
            await EnsureAtLeastOneChainAdminRemainsAsync();
        }

        var previousRole = target.Role;
        target.AssignRole(command.NewRole, actor.Id, timeProvider.GetUtcNow());

        // US-03 scenario 2: the new role ends every session of the user (they sign in again with it).
        if (target.Role != previousRole)
            await EndSessionsAsync(target, notifyUser: true);
        else
            await unitOfWork.CompleteAsync();
    }

    /// <summary>
    /// Deactivates a user account (soft delete).
    /// Actor must have hierarchy superiority and scope access over the target.
    /// </summary>
    public async Task Handle(DeactivateUserCommand command)
    {
        var actor = await ResolveActorAsync(command.ActorUserId);
        var target = await ResolveTargetAsync(command.TargetUserId);

        if (!roleAuthorizationService.CanManage(actor, target))
            throw new UnauthorizedOperationException(IamErrorCodes.OutsideHierarchy,
                $"User {actor.Id} cannot deactivate user {target.Id}.");

        // --- NEW RULE: Protect the last ChainAdmin ---
        if (string.Equals(target.Role.Value, UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase) &&
            target.Status == UserStatus.Active)
        {
            await EnsureAtLeastOneChainAdminRemainsAsync();
        }

        var now = timeProvider.GetUtcNow();
        target.Deactivate(actor.Id, now);
        foreach (var session in await refreshTokenRepository.ListUnrevokedByUserAsync(target.Id))
            session.Revoke(RefreshTokenRevocationReason.SessionRevoked, now);
        await unitOfWork.CompleteAsync();
    }

    /// <summary>
    ///     Activates a previously deactivated user account (reverse soft delete).
    ///     Actor must have hierarchy superiority and scope access over the target.
    /// </summary>
    public async Task Handle(ActivateUserCommand command)
    {
        var actor = await ResolveActorAsync(command.ActorUserId);
        var target = await ResolveTargetAsync(command.TargetUserId);

        if (!roleAuthorizationService.CanManage(actor, target))
            throw new UnauthorizedOperationException(IamErrorCodes.OutsideHierarchy,
                $"User {actor.Id} cannot activate user {target.Id}.");

        target.Activate(actor.Id, timeProvider.GetUtcNow());
        await unitOfWork.CompleteAsync();
    }

    /// <summary>
    ///     D2: the hotel registered by a hotel administrator becomes the hotel they administer.
    /// </summary>
    public async Task<AuthenticationResult?> Handle(AssignHotelToAdministratorCommand command)
    {
        var user = await ResolveTargetAsync(command.UserId);
        var now = timeProvider.GetUtcNow();
        var previousHotel = user.HotelId;
        // Read before the sessions end: was the session that registered the hotel a remembered one of this user?
        var remembered = command.RememberedSessionId is { } sessionId
                         && (await refreshTokenRepository.ListUnrevokedByFamilyAsync(sessionId))
                         .Any(token => token.UserId == user.Id && token.IsActive(now));

        user.TakeChargeOfHotel(command.HotelId, now);
        if (user.HotelId == previousHotel)
        {
            await unitOfWork.CompleteAsync();
            return null;
        }

        // OWASP, privilege change: the tokens without the hotel stop working (new session generation)...
        await EndSessionsAsync(user, notifyUser: false);
        // ...and, since the administrator asked for the change, new credentials with the hotel are issued at once
        // (no second sign-in and MFA just to pick up their own change).
        return await sessionIssuer.ReissueSessionAsync(user, remembered, now);
    }

    /// <summary>
    ///     Commits a change that started a new session generation of <paramref name="user"/>: the remembered
    ///     sessions are revoked with it (access tokens already fail the version check) and, when an administrator
    ///     made the change, the user is told by e-mail to sign in again.
    /// </summary>
    private async Task EndSessionsAsync(User user, bool notifyUser)
    {
        var now = timeProvider.GetUtcNow();
        foreach (var session in await refreshTokenRepository.ListUnrevokedByUserAsync(user.Id))
            session.Revoke(RefreshTokenRevocationReason.SessionRevoked, now);
        await unitOfWork.CompleteAsync();

        if (notifyUser)
            await notifications.SendPermissionsChangedAsync(user);
    }

    // ═══════════════════════════════════════════════════════════
    // Private helpers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves the actor user and validates they are active.
    /// </summary>
    private async Task<User> ResolveActorAsync(int actorUserId)
    {
        var actor = await userRepository.FindByIdAsync(actorUserId);
        if (actor == null)
            throw new UserNotFoundException(actorUserId);

        if (actor.Status == UserStatus.Inactive)
            throw new UnauthorizedOperationException(IamErrorCodes.ActorInactive,
                $"User {actorUserId} is inactive and cannot perform management operations.");

        return actor;
    }

    /// <summary>
    /// Resolves the target user for management operations.
    /// </summary>
    private async Task<User> ResolveTargetAsync(int targetUserId)
    {
        var target = await userRepository.FindByIdAsync(targetUserId);
        if (target == null)
            throw new UserNotFoundException(targetUserId);

        return target;
    }

    /// <summary>
    /// Ensures that at least one active ChainAdmin remains in the system.
    /// </summary>
    private async Task EnsureAtLeastOneChainAdminRemainsAsync()
    {
        var activeChainAdminsCount = await userRepository.CountActiveByRoleAsync(UserRoles.ChainAdmin);
        if (activeChainAdminsCount <= 1)
        {
            throw new UnauthorizedOperationException(IamErrorCodes.LastChainAdmin, "Operation would leave the system without an active ChainAdmin.");
        }
    }
}