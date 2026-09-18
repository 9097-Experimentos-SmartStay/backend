using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Configuration;

/// <summary>
///     JWT settings bound from the <c>TokenSettings</c> section (env vars <c>TokenSettings__*</c>).
///     Validated at startup: the application refuses to start without a strong signing secret.
/// </summary>
public class TokenSettings : IValidatableObject
{
    public const string SectionName = "TokenSettings";

    /// <summary>Minimum secret length in bytes accepted for HS256 signing keys.</summary>
    public const int MinimumSecretLength = 32;

    /// <summary>HS256 signing secret. Never commit it: set <c>TokenSettings__Secret</c>.</summary>
    [Required(ErrorMessage = "TokenSettings:Secret is not configured. Set the 'TokenSettings__Secret' environment variable (at least 32 random bytes).")]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     Lifetime of an access token. Short on purpose (US-02): "remember me" sessions last through refresh tokens.
    /// </summary>
    [Range(1, 24 * 60)]
    public int AccessTokenExpirationMinutes { get; set; } = 30;

    /// <summary>Lifetime of a second-factor challenge token (time to scan the QR code or type the code, US-52).</summary>
    [Range(1, 60)]
    public int MfaChallengeTokenExpirationMinutes { get; set; } = 10;

    /// <summary>Audience of the challenge tokens: different from <see cref="Audience"/>, so they are never access tokens.</summary>
    public string MfaChallengeAudience => $"{Audience}/mfa-challenge";

    /// <summary>Tolerated clock difference between the issuer and the validator.</summary>
    [Range(0, 300)]
    public int ClockSkewSeconds { get; set; } = 30;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(Secret));

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Secret) && Encoding.UTF8.GetByteCount(Secret) < MinimumSecretLength)
            yield return new ValidationResult(
                $"TokenSettings:Secret is too short. HS256 requires at least {MinimumSecretLength} bytes.",
                [nameof(Secret)]);
    }
}
