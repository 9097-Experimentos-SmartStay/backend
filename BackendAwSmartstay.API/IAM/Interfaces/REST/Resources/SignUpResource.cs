using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource definition for user registration with e-mail and password (US-01).
/// </summary>
public record SignUpResource : IValidatableObject
{
    /// <summary>The account e-mail (login identifier).</summary>
    [EmailAddress]
    [MaxLength(254)]
    public string? Email { get; init; }

    /// <summary>Deprecated alias of <see cref="Email"/>, accepted for backward compatibility.</summary>
    [EmailAddress]
    [MaxLength(254)]
    public string? Username { get; init; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; init; } = string.Empty;

    /// <summary>Optional role. Anything but <c>guest</c> needs the token of a user allowed to assign it.</summary>
    public string? Role { get; init; }

    [JsonIgnore]
    public string LoginEmail => (string.IsNullOrWhiteSpace(Email) ? Username : Email) ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail))
            yield return new ValidationResult("The Email field is required.", [nameof(Email)]);
    }
}
