using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using Microsoft.AspNetCore.DataProtection;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Security;

/// <summary>Adapter of <see cref="ISecretProtector"/> on ASP.NET Core Data Protection (AES-256-CBC + HMAC-SHA256, rotated keys).</summary>
public class DataProtectionSecretProtector(IDataProtectionProvider dataProtectionProvider) : ISecretProtector
{
    public string Protect(string purpose, string plaintext) => Protector(purpose).Protect(plaintext);

    public string Unprotect(string purpose, string protectedValue) => Protector(purpose).Unprotect(protectedValue);

    public byte[] Protect(string purpose, byte[] plaintext) => Protector(purpose).Protect(plaintext);

    public byte[] Unprotect(string purpose, byte[] protectedValue) => Protector(purpose).Unprotect(protectedValue);

    private IDataProtector Protector(string purpose) => dataProtectionProvider.CreateProtector("SmartStay", purpose);
}
