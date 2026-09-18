namespace BackendAwSmartstay.API.IAM.Infrastructure.Seed;

/// <summary>
///     Credentials of the first chain administrator (<c>InitialChainAdmin__Username</c> /
///     <c>InitialChainAdmin__Password</c>). Optional: without them the seed is skipped.
/// </summary>
public class InitialChainAdminSettings
{
    public const string SectionName = "InitialChainAdmin";

    public string? Username { get; set; }
    public string? Password { get; set; }

    /// <summary>Hotel assigned to the seeded chain administrator.</summary>
    public int? HotelId { get; set; } = 1;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
