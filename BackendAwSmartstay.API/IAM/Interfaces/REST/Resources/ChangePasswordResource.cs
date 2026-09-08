using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource representing the payload for a password change request.
/// </summary>
public record ChangePasswordResource
{
    /// <summary>
    ///     The user's current password.
    /// </summary>
    [Required]
    public required string CurrentPassword { get; init; }

    /// <summary>
    ///     The desired new password.
    /// </summary>
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string NewPassword { get; init; }
}