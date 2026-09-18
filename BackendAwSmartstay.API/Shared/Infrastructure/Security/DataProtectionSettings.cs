namespace BackendAwSmartstay.API.Shared.Infrastructure.Security;

/// <summary>
///     ASP.NET Core Data Protection (section <c>DataProtection</c>, env vars <c>DataProtection__*</c>).
/// </summary>
public class DataProtectionSettings
{
    public const string SectionName = "DataProtection";

    /// <summary>Isolates the key ring: every instance of the API must use the same name.</summary>
    public string ApplicationName { get; set; } = "SmartStay";

    /// <summary>
    ///     Optional PKCS#12 (.pfx) certificate, base64, that encrypts the keys stored in the database. Without it
    ///     the keys are stored unencrypted in the <c>data_protection_keys</c> table.
    /// </summary>
    public string? CertificatePfxBase64 { get; set; }

    /// <summary>Password of <see cref="CertificatePfxBase64"/>.</summary>
    public string? CertificatePassword { get; set; }
}
