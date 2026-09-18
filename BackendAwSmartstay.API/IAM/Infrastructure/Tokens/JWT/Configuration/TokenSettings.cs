namespace BackendAwSmartstay.API.IAM.Infrastructure.Tokens.JWT.Configuration;

/**
 * <summary>
 *     This class is used to store the token settings.
 *     It is used to configure the token settings in the app settings .json file.
 * </summary>
 */

public class TokenSettings
{
    /// <summary>Minimum secret length in bytes accepted for HS256 signing keys.</summary>
    public const int MinimumSecretLength = 32;

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInHours { get; set; } = 24;
}