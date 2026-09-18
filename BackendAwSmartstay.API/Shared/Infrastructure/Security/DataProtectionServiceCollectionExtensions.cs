using System.Security.Cryptography.X509Certificates;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.AspNetCore.DataProtection;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Security;

public static class DataProtectionServiceCollectionExtensions
{
    /// <summary>
    ///     Data Protection with its key ring persisted in the application database.
    /// </summary>
    /// <remarks>
    ///     The default key ring lives in the container file system (<c>~/.aspnet/DataProtection-Keys</c>). On Render
    ///     every deploy or restart starts a fresh container, so the keys would be lost and every stored MFA secret and
    ///     identity document would become unreadable (all staff locked out of MFA). The <c>data_protection_keys</c>
    ///     table survives restarts and is shared by every instance. Optionally the keys are encrypted at rest with a
    ///     certificate kept outside the database (<see cref="DataProtectionSettings.CertificatePfxBase64"/>).
    /// </remarks>
    public static IServiceCollection AddSmartStayDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(DataProtectionSettings.SectionName).Get<DataProtectionSettings>()
                       ?? new DataProtectionSettings();

        var dataProtection = services.AddDataProtection()
            .SetApplicationName(string.IsNullOrWhiteSpace(settings.ApplicationName) ? "SmartStay" : settings.ApplicationName)
            .PersistKeysToDbContext<AppDbContext>();

        // An invalid certificate fails at startup (fail fast) instead of when the first secret is read.
        if (!string.IsNullOrWhiteSpace(settings.CertificatePfxBase64))
            dataProtection.ProtectKeysWithCertificate(X509CertificateLoader.LoadPkcs12(
                Convert.FromBase64String(settings.CertificatePfxBase64), settings.CertificatePassword));

        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
        return services;
    }
}
