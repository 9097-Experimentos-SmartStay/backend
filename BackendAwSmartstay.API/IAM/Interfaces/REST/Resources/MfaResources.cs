using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;
using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>What an authenticator app needs to add the account (US-52 scenario 1).</summary>
/// <param name="Secret">The shared secret in Base32, for manual entry.</param>
/// <param name="OtpAuthUri">The <c>otpauth://totp/...</c> URI to render as a QR code.</param>
/// <param name="Issuer">Name shown by the app (SmartStay).</param>
/// <param name="AccountName">The account e-mail shown by the app.</param>
/// <param name="Digits">Code length (6).</param>
/// <param name="Period">Seconds each code lasts (30).</param>
/// <param name="Algorithm">HMAC algorithm (SHA1).</param>
public record MfaEnrollmentResource(string Secret, string OtpAuthUri, string Issuer, string AccountName, int Digits, int Period, string Algorithm);

/// <summary>A 6-digit code of the authenticator app.</summary>
public record MfaCodeResource
{
    /// <summary>The code shown by the authenticator app (spaces are ignored).</summary>
    /// <example>492039</example>
    [Required]
    [RegularExpression(@"^\s*\d{3}\s?\d{3}\s*$", ErrorMessage = "Enter the 6-digit code of your authenticator app.")]
    public string? Code { get; init; }
}

/// <summary>The second factor: a code of the authenticator app or one recovery code (exactly one).</summary>
public record MfaVerificationResource : IValidatableObject
{
    /// <summary>The 6-digit code of the authenticator app.</summary>
    /// <example>492039</example>
    [RegularExpression(@"^\s*\d{3}\s?\d{3}\s*$", ErrorMessage = "Enter the 6-digit code of your authenticator app.")]
    public string? Code { get; init; }

    /// <summary>A one-time recovery code (<c>XXXXX-XXXXX</c>), when the phone is not at hand.</summary>
    /// <example>K7M2Q-9TRHX</example>
    [MaxLength(40)]
    public string? RecoveryCode { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasCode = !string.IsNullOrWhiteSpace(Code);
        var hasRecovery = !string.IsNullOrWhiteSpace(RecoveryCode);
        if (hasCode == hasRecovery)
            yield return new CodedValidationResult(IamErrorCodes.MfaCodeOrRecoveryCodeRequired,
                "Send either the code of your authenticator app or a recovery code.", ["code"]);
        else if (hasRecovery && !RecoveryCodeFormat.IsWellFormed(RecoveryCode))
            yield return new CodedValidationResult(IamErrorCodes.MfaRecoveryCodeFormat,
                "A recovery code has 10 letters and digits (XXXXX-XXXXX).", ["recoveryCode"]);
    }
}
