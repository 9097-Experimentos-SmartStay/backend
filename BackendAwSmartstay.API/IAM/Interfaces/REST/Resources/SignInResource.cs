using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BackendAwSmartstay.API.IAM.Interfaces.REST.Validation;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Credentials to sign in with the account e-mail (US-02).
/// </summary>
public record SignInResource : IValidatableObject
{
    /// <summary>The account e-mail.</summary>
    /// <example>chain.admin@smartstay.pe</example>
    [AccountEmail]
    [MaxLength(254)]
    public string? Email { get; init; }

    /// <summary>Deprecated alias of <see cref="Email"/>, accepted for backward compatibility.</summary>
    [AccountEmail]
    [MaxLength(254)]
    public string? Username { get; init; }

    /// <summary>The account password.</summary>
    /// <example>ChainAdmin#2026</example>
    [Required]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    ///     "Remember me" (US-02 scenario 4): also return a refresh token so the client stays signed in until the
    ///     user signs out. Without it only the short-lived access token is returned.
    /// </summary>
    /// <example>true</example>
    public bool RememberMe { get; init; }

    /// <summary>The login e-mail: <see cref="Email"/>, or the legacy <see cref="Username"/> field.</summary>
    [JsonIgnore]
    public string LoginEmail => (string.IsNullOrWhiteSpace(Email) ? Username : Email) ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail))
            yield return new CodedValidationResult(ErrorCodes.FieldRequired, "The Email field is required.", ["email"]);
    }
}
