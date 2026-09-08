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
    ITokenService tokenService,
    IHashingService hashingService,
    IRoleAuthorizationService roleAuthorizationService,
    IUserScopeService userScopeService,
    IUnitOfWork unitOfWork) : IUserCommandService
{
    // ═══════════════════════════════════════════════════════════
    // Existing authentication commands
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Processes a sign-in request.
    /// </summary>
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var username = new Username(command.Username);
        var user = await userRepository.FindByUsernameAsync(username);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        if (user.Status == UserStatus.Inactive)
            throw new UnauthorizedOperationException("La cuenta ha sido desactivada. Contacte al administrador.");

        var token = tokenService.GenerateToken(user);
        return (user, token);
    }

    /// <summary>
    /// Processes a sign-up request with optional role assignment.
    /// Guest registration remains anonymous even if explicitly requested.
    /// </summary>
    public async Task Handle(SignUpCommand command)
    {
        var username = new Username(command.Username);
        if (await userRepository.ExistsByUsernameAsync(username))
            throw new UsernameAlreadyExistsException(command.Username);

        var hashedPassword = hashingService.HashPassword(command.Password);
        var assignedRole = UserRoles.Guest;

        // If a specific role is requested and it is not the default Guest role, validate the actor's permissions
        if (!string.IsNullOrWhiteSpace(command.Role) 
            && !command.Role.Equals(UserRoles.Guest, StringComparison.OrdinalIgnoreCase))
        {
            if (command.ActorUserId == null)
                throw new UnauthorizedOperationException("Authentication required to assign a specific role during sign-up.");

            var actor = await ResolveActorAsync(command.ActorUserId.Value);

            if (!roleAuthorizationService.CanAssignRole(actor, command.Role))
                throw new UnauthorizedOperationException(
                    $"User {actor.Id} cannot assign role '{command.Role}'.");

            assignedRole = command.Role;
        }

        // HotelId and ChainId default to null via the constructor logic.
        // For full scope initialization, management endpoints (CreateUser) should be used.
        var user = new User(username, hashedPassword, assignedRole);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
    }

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

        var newHashedPassword = hashingService.HashPassword(command.NewPassword);
        user.UpdatePasswordHash(newHashedPassword);
        user.IncrementTokenVersion();

        await unitOfWork.CompleteAsync();
    }

    // ═══════════════════════════════════════════════════════════
    // New management commands
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new user with explicit role, hotel and chain assignment.
    /// Only actors with hierarchy superiority and scope access can create users.
    /// </summary>
    public async Task Handle(CreateUserCommand command)
    {
        var actor = await ResolveActorAsync(command.ActorUserId);

        if (!roleAuthorizationService.CanAssignRole(actor, command.Role))
            throw new UnauthorizedOperationException(
                $"User {actor.Id} cannot assign role '{command.Role}'.");

        if (command.HotelId.HasValue && !userScopeService.CanAccessHotel(actor, command.HotelId))
            throw new UnauthorizedOperationException(
                $"User {actor.Id} cannot create users for hotel {command.HotelId}.");

        if (command.ChainId.HasValue && !roleAuthorizationService.CanAssignChainId(actor, command.ChainId))
            throw new UnauthorizedOperationException(
                $"User {actor.Id} cannot assign chain {command.ChainId}.");

        var username = new Username(command.Username);
        if (await userRepository.ExistsByUsernameAsync(username))
            throw new UsernameAlreadyExistsException(command.Username);

        var hashedPassword = hashingService.HashPassword(command.Password);
        var user = new User(
            command.Username,
            hashedPassword,
            command.Role,
            hotelId: command.HotelId,
            chainId: command.ChainId);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
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

        if (command.NewUsername is not null)
        {
            var newUsername = new Username(command.NewUsername);
            if (!string.Equals(target.Username.Value, newUsername.Value, StringComparison.OrdinalIgnoreCase)
                && await userRepository.ExistsByUsernameAsync(newUsername))
            {
                throw new UsernameAlreadyExistsException(command.NewUsername);
            }
            target.UpdateUsername(command.NewUsername);
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

        target.AssignRole(command.NewRole);
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

        target.Deactivate();
        target.IncrementTokenVersion();
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

        target.Activate();
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