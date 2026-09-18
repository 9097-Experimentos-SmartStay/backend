namespace BackendAwSmartstay.API.IAM.Infrastructure.Seed;

/// <summary>
///     Credentials of the first chain administrator (<c>InitialChainAdmin__Email</c> /
///     <c>InitialChainAdmin__Password</c>; <c>InitialChainAdmin__Username</c> is a deprecated alias of the
///     e-mail). Optional: without them the seed is skipped.
/// </summary>
public class InitialChainAdminSettings
{
    public const string SectionName = "InitialChainAdmin";

    public string? Email { get; set; }

    /// <summary>Deprecated alias of <see cref="Email"/>.</summary>
    public string? Username { get; set; }

    public string? LoginEmail => string.IsNullOrWhiteSpace(Email) ? Username : Email;
    public string? Password { get; set; }

    /// <summary>Hotel assigned to the seeded chain administrator.</summary>
    public int? HotelId { get; set; } = 1;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(LoginEmail) && !string.IsNullOrWhiteSpace(Password);
}
