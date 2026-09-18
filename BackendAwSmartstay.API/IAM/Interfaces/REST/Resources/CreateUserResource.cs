using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Validation;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// A user created by an administrator (US-03 scenario 1), typically a staff member with a role.
/// </summary>
public record CreateUserResource : IValidatableObject
{
    /// <summary>First name.</summary>
    /// <example>Rosa</example>
    [Required]
    [PersonNamePart]
    public string? FirstName { get; init; }

    /// <summary>Last name.</summary>
    /// <example>Quispe</example>
    [Required]
    [PersonNamePart]
    public string? LastName { get; init; }

    /// <summary>The account e-mail (login identifier). A verification link is sent to it.</summary>
    /// <example>rosa.quispe@smartstay.pe</example>
    [AccountEmail]
    [MaxLength(254)]
    public string? Email { get; init; }

    /// <summary>Deprecated alias of <see cref="Email"/>, accepted for backward compatibility.</summary>
    [AccountEmail]
    [MaxLength(254)]
    public string? Username { get; init; }

    /// <summary>Initial password (8 to 128 characters); the user can change it or reset it later.</summary>
    /// <example>Temporal#2026</example>
    [Required]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "The password must have between 8 and 128 characters.")]
    public string Password { get; init; } = string.Empty;

    /// <summary>reception, housekeeping or maintenance (admin can assign them); chain_admin can also assign admin.</summary>
    /// <example>housekeeping</example>
    [Required]
    public string Role { get; init; } = string.Empty;

    /// <summary>Hotel of the user. Optional for a hotel admin (always their hotel); required for staff created by a chain admin.</summary>
    /// <example>1</example>
    public int? HotelId { get; init; }

    /// <summary>Chain of the user (only a global chain admin can set it).</summary>
    public int? ChainId { get; init; }

    [JsonIgnore]
    public string LoginEmail => (string.IsNullOrWhiteSpace(Email) ? Username : Email) ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail))
            yield return new ValidationResult("The Email field is required.", ["email"]);
    }
}
