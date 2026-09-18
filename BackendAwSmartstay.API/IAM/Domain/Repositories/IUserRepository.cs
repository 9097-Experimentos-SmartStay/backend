using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
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
    ///     Find a user by e-mail
    /// </summary>
    Task<User?> FindByEmailAsync(Email email);

    /// <summary>
    ///     Checks asynchronously whether a user with the given username exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(Email email);

    /// <summary>
    ///     Counts the number of active users that have a specific role.
    /// </summary>
    Task<int> CountActiveByRoleAsync(string role);

    /// <summary>Active users holding one of <paramref name="roles"/>.</summary>
    Task<IReadOnlyList<User>> ListActiveByRolesAsync(IReadOnlyCollection<Role> roles);
}