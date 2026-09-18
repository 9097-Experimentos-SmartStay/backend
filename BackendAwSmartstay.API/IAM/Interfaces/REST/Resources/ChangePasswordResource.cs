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
    [MaxLength(256, ErrorMessage = "The password cannot exceed 128 characters.")]
    public required string NewPassword { get; init; }
}