using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Application.Internal.Configuration;

/// <summary>Two-factor authentication (section <c>Mfa</c>, env vars <c>Mfa__*</c>, US-52). Validated at startup.</summary>
public class MfaSettings
{
    public const string SectionName = "Mfa";

    /// <summary>Name shown by the authenticator app next to the account (otpauth <c>issuer</c>).</summary>
    [Required, StringLength(40, MinimumLength = 2)]
    [RegularExpression("^[^:]+$", ErrorMessage = "Mfa:Issuer cannot contain ':'.")]
    public string Issuer { get; set; } = "SmartStay";
}
