using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Passwords;

/// <summary>
///     Breached password lookup (section <c>PasswordPolicy</c>, env vars <c>PasswordPolicy__*</c>). Validated at startup.
/// </summary>
public class PasswordPolicySettings
{
    public const string SectionName = "PasswordPolicy";

    /// <summary>Look new passwords up in Have I Been Pwned (k-anonymity). Disable only offline.</summary>
    public bool BreachedPasswordCheckEnabled { get; set; } = true;

    /// <summary>Base URL of the Pwned Passwords range API.</summary>
    [Required, Url]
    public string PwnedPasswordsApiBaseUrl { get; set; } = "https://api.pwnedpasswords.com/";

    /// <summary>Timeout of one lookup; afterwards the password is accepted (fail open) and a warning is logged.</summary>
    [Range(1, 30)]
    public int PwnedPasswordsTimeoutSeconds { get; set; } = 3;
}
