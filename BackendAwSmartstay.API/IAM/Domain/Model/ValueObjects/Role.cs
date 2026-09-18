using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
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
        { UserRoles.Reception, 1 },
        { UserRoles.Housekeeping, 1 },
        { UserRoles.Maintenance, 1 },
        { UserRoles.Admin, 2 },
        { UserRoles.ChainAdmin, 3 }
    };

    public Role(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException(IamErrorCodes.RoleRequired, "Role cannot be empty or whitespace.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!RoleHierarchy.ContainsKey(normalized))
            throw new DomainValidationException(IamErrorCodes.RoleUnknown,
                $"Invalid role: '{value}'. Allowed roles are: {string.Join(", ", RoleHierarchy.Keys)}.");

        // Canonical lowercase value: role claims and policies compare it exactly.
        Value = normalized;
        HierarchyLevel = RoleHierarchy[normalized];
    }

    /// <summary>
    /// Returns true if this role is strictly higher in hierarchy than the other.
    /// </summary>
    public bool CanManage(Role other) => HierarchyLevel > other.HierarchyLevel;

    /// <summary>
    /// Returns true if this role is higher or equal in hierarchy than the other.
    /// </summary>
    public bool CanManageOrEqual(Role other) => HierarchyLevel >= other.HierarchyLevel;

    /// <summary>True for hotel staff roles (reception, housekeeping, maintenance, admin, chain_admin).</summary>
    public bool IsStaff => Value != UserRoles.Guest;

    /// <summary>
    ///     Staff accounts must use a second factor (TOTP, US-52). Guests may not (NIST SP 800-63B-4: a single-factor
    ///     password must be longer, see <c>PasswordPolicy</c>).
    /// </summary>
    public bool RequiresMultiFactorAuthentication => IsStaff;

    public static implicit operator string(Role role) => role.Value;
    public static implicit operator Role(string value) => new(value);

    public override string ToString() => Value;
}