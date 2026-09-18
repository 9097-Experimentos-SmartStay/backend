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
            throw new UnauthorizedOperationException($"You cannot assign the role '{role.Value}'.");

        var hotelId = StaffAccountPolicy.ResolveHotel(actor, role.Value, command.HotelId);
        if (hotelId.HasValue && !userScopeService.CanAccessHotel(actor, hotelId))
            throw new UnauthorizedOperationException($"You cannot create users for hotel {hotelId}.");

        if (command.ChainId.HasValue && !roleAuthorizationService.CanAssignChainId(actor, command.ChainId))
            throw new UnauthorizedOperationException($"You cannot assign chain {command.ChainId}.");

        var email = new Email(command.Email);
        if (await userRepository.ExistsByEmailAsync(email))
            throw new EmailAlreadyRegisteredException(email.Value);

        var name = new PersonName(command.FirstName, command.LastName);
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
            throw new UnauthorizedOperationException(
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
            var hashed = hashingService.HashPassword(command.NewPassword);
            target.UpdatePasswordHash(hashed);
        }

        if (command.NewHotelId.HasValue)
        {
            if (!userScopeService.CanAccessHotel(actor, command.NewHotelId))
                throw new UnauthorizedOperationException(
                    $"User {actor.Id} cannot assign hotel {command.NewHotelId}.");

            target.UpdateHotelId(command.NewHotelId);
        }

        if (command.NewChainId.HasValue)
        {
            if (!roleAuthorizationService.CanAssignChainId(actor, command.NewChainId))
                throw new UnauthorizedOperationException(
                    $"User {actor.Id} cannot assign chain {command.NewChainId}.");

            target.UpdateChainId(command.NewChainId);
        }

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
            throw new UnauthorizedOperationException(
                $"User {actor.Id} cannot manage user {target.Id}.");

        if (!roleAuthorizationService.CanAssignRole(actor, command.NewRole))
            throw new UnauthorizedOperationException(
                $"User {actor.Id} cannot assign role '{command.NewRole}'.");

        // --- NEW RULE: Protect the last ChainAdmin ---
        if (string.Equals(target.Role.Value, UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(command.NewRole, UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase) &&
            target.Status == UserStatus.Active)
        {
            await EnsureAtLeastOneChainAdminRemainsAsync();
        }

        target.AssignRole(command.NewRole, actor.Id, timeProvider.GetUtcNow());
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
            throw new UnauthorizedOperationException(
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
            throw new UnauthorizedOperationException(
                $"User {actor.Id} cannot activate user {target.Id}.");

        target.Activate(actor.Id, timeProvider.GetUtcNow());
        await unitOfWork.CompleteAsync();
    }

    /// <summary>
    ///     D2: the hotel registered by a hotel administrator becomes the hotel they administer.
    /// </summary>
    public async Task Handle(AssignHotelToAdministratorCommand command)
    {
        var user = await ResolveTargetAsync(command.UserId);
        user.TakeChargeOfHotel(command.HotelId);
        await unitOfWork.CompleteAsync();
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
            throw new UnauthorizedOperationException(
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
            throw new UnauthorizedOperationException("Operation would leave the system without an active ChainAdmin.");
        }
    }
}