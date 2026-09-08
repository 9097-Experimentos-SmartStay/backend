using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Domain.Repositories;

/// <summary>
///     The user repository
/// </summary>
/// <remarks>
///     This repository is used to manage users
/// </remarks>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    ///     Find a user by username
    /// </summary>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>
    ///     Checks asynchronously whether a user with the given username exists.
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username);

    /// <summary>
    ///     Counts the number of active users that have a specific role.
    /// </summary>
    Task<int> CountActiveByRoleAsync(string role);
}