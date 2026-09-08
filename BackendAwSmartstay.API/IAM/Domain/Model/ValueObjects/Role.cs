using BackendAwSmartstay.API.IAM.Domain.Model.Constants;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
/// Value Object that encapsulates role identity and hierarchy behavior.
/// </summary>
public sealed record Role
{
    public string Value { get; }
    public int HierarchyLevel { get; }

    private static readonly Dictionary<string, int> RoleHierarchy = new(StringComparer.OrdinalIgnoreCase)
    {
        { UserRoles.Guest, 0 },
        { UserRoles.Staff, 1 },
        { UserRoles.Reception, 1 },
        { UserRoles.Housekeeping, 1 },
        { UserRoles.Maintenance, 1 },
        { UserRoles.Admin, 2 },
        { UserRoles.ChainAdmin, 3 }
    };

    public Role(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Role cannot be empty or whitespace.", nameof(value));

        if (!RoleHierarchy.ContainsKey(value))
            throw new ArgumentException(
                $"Invalid role: '{value}'. Allowed roles are: {string.Join(", ", RoleHierarchy.Keys)}.",
                nameof(value));

        Value = value;
        HierarchyLevel = RoleHierarchy[value];
    }

    /// <summary>
    /// Returns true if this role is strictly higher in hierarchy than the other.
    /// </summary>
    public bool CanManage(Role other) => HierarchyLevel > other.HierarchyLevel;

    /// <summary>
    /// Returns true if this role is higher or equal in hierarchy than the other.
    /// </summary>
    public bool CanManageOrEqual(Role other) => HierarchyLevel >= other.HierarchyLevel;

    public static implicit operator string(Role role) => role.Value;
    public static implicit operator Role(string value) => new(value);

    public override string ToString() => Value;
}