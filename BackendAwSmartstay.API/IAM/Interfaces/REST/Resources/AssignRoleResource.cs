using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for reassigning a user's role.
/// </summary>
public record AssignRoleResource(
    [Required] int TargetUserId,
    [Required] string NewRole
);