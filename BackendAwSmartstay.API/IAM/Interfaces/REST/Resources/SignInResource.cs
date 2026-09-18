using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource for signing in a user with their e-mail (US-02).
/// </summary>
public record SignInResource : IValidatableObject
{
    /// <summary>The account e-mail.</summary>
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

    /// <summary>The login e-mail: <see cref="Email"/>, or the legacy <see cref="Username"/> field.</summary>
    [JsonIgnore]
    public string LoginEmail => (string.IsNullOrWhiteSpace(Email) ? Username : Email) ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail))
            yield return new ValidationResult("The Email field is required.", [nameof(Email)]);
    }
}
