using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>
/// Implementation of role-hierarchy authorization rules.
/// </summary>
public class RoleAuthorizationService(IUserScopeService scopeService) : IRoleAuthorizationService
{
    public bool CanManage(User manager, User managed)
    {
        if (manager == null || managed == null) return false;

        // A user cannot manage themselves
        if (manager.Id == managed.Id) return false;

        // Hierarchy: manager must be strictly higher in rank
        if (manager.Role.HierarchyLevel <= managed.Role.HierarchyLevel)
            return false;

        // Scope: manager must be allowed to access the target user's data
        return scopeService.CanAccessUser(manager, managed);
    }

    public bool CanAssignRole(User assigner, string targetRole)
    {
        if (assigner == null || string.IsNullOrWhiteSpace(targetRole))
            return false;

        // The Role value object validates the string and exposes its hierarchy level
        var role = new Role(targetRole);

        // Cannot assign a role equal or higher than one's own
        if (assigner.Role.HierarchyLevel <= role.HierarchyLevel)
            return false;

        // ChainAdmin can only be assigned by a global ChainAdmin (ChainId == null)
        if (role.Value.Equals(UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return assigner.Role.Value.Equals(UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase)
                   && assigner.ChainId == null;
        }

        return true;
    }

    /// <summary>
    /// Only a global ChainAdmin (ChainId == null) can assign or reassign ChainId.
    /// All other roles are forbidden from modifying chain affiliation.
    /// </summary>
    public bool CanAssignChainId(User assigner, int? targetChainId)
    {
        if (assigner == null) return false;

        return assigner.Role.Value.Equals(UserRoles.ChainAdmin, StringComparison.OrdinalIgnoreCase)
               && assigner.ChainId == null;
    }
}