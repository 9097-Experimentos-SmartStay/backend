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
     *     Handle sign in command
     * </summary>
     * <param name="command">The sign in command</param>
     * <returns>The authenticated user and the JWT token</returns>
     */
    Task<(User user, string token)> Handle(SignInCommand command);

    /**
     * <summary>
     *     Handle sign up command
     * </summary>
     * <param name="command">The sign-up command</param>
     * <returns>A confirmation message on successful creation.</returns>
     */
    Task Handle(SignUpCommand command);

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
    Task Handle(CreateUserCommand command);

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
}