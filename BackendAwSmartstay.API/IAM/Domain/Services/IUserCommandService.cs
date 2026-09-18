using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/**
 * <summary>
 *     The user command service
 * </summary>
 * <remarks>
 *     This interface is used to handle user commands
 * </remarks>
 */
public interface IUserCommandService
{
    /**
     * <summary>
     *     Handle change password command
     * </summary>
     * <param name="command">The change password command</param>
     */
    Task Handle(ChangePasswordCommand command);

    /// <summary>
    ///     Handle create user command (admin/chain_admin scoped).
    /// </summary>
    Task<User> Handle(CreateUserCommand command);

    /// <summary>
    ///     Handle update user command (admin/chain_admin scoped).
    /// </summary>
    Task Handle(UpdateUserCommand command);

    /// <summary>
    ///     Handle assign role command (admin/chain_admin scoped).
    /// </summary>
    Task Handle(AssignRoleCommand command);

    /// <summary>
    ///     Handle deactivate user command — performs soft delete.
    /// </summary>
    Task Handle(DeactivateUserCommand command);

    /// <summary>
    ///     Handle activate user command — reverses a soft delete.
    /// </summary>
    Task Handle(ActivateUserCommand command);

    /// <summary>
    ///     Handle assign hotel to administrator command (D2: the hotel an admin registers becomes their hotel).
    ///     The change was asked by the administrator: every previous session ends and a new one is returned.
    /// </summary>
    /// <returns>The new session with the hotel, or null when the administrator already had that hotel.</returns>
    Task<AuthenticationResult?> Handle(AssignHotelToAdministratorCommand command);
}