using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Validation;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Registration data (US-01). Every field but <see cref="Role"/> is required; missing or malformed fields are
///     reported one by one in <c>errors</c> (US-01 scenarios 3 and 4).
/// </summary>
public record SignUpResource : IValidatableObject
{
    /// <summary>First name (letters, 2 to 50 characters).</summary>
    /// <example>Ana</example>
    [Required]
    [PersonNamePart]
    public string? FirstName { get; init; }

    /// <summary>Last name (letters, 2 to 50 characters).</summary>
    /// <example>Pérez</example>
    [Required]
    [PersonNamePart]
    public string? LastName { get; init; }

    /// <summary>The account e-mail (login identifier). A verification link is sent to it.</summary>
    /// <example>ana.perez@example.com</example>
    [AccountEmail]
    [MaxLength(254)]
    public string? Email { get; init; }

    /// <summary>Deprecated alias of <see cref="Email"/>, accepted for backward compatibility.</summary>
    [AccountEmail]
    [MaxLength(254)]
    public string? Username { get; init; }

    /// <summary>Password, 8 to 128 characters.</summary>
    /// <example>Secreta#2026</example>
    [Required]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "The password must have between 8 and 128 characters.")]
    public string? Password { get; init; }

    /// <summary>Optional role. Anything but <c>guest</c> needs the token of a user allowed to assign it.</summary>
    /// <example>guest</example>
    public string? Role { get; init; }

    [JsonIgnore]
    public string LoginEmail => (string.IsNullOrWhiteSpace(Email) ? Username : Email) ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail))
            yield return new ValidationResult("The Email field is required.", [nameof(Email)]);
    }
}
