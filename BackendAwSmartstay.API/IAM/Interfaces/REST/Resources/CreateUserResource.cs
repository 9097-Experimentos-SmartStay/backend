using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for creating a new user via management endpoints.
/// </summary>
public record CreateUserResource : IValidatableObject
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

    [Required]
    public string Role { get; init; } = string.Empty;

    public int? HotelId { get; init; }

    public int? ChainId { get; init; }

    [JsonIgnore]
    public string LoginEmail => (string.IsNullOrWhiteSpace(Email) ? Username : Email) ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail))
            yield return new ValidationResult("The Email field is required.", [nameof(Email)]);
    }
}
