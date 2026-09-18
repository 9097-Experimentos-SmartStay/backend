namespace BackendAwSmartstay.API.Shared.Application.OutboundServices;

/// <summary>
///     Encrypts secrets that must be stored but read back later (MFA secrets, identity document images).
///     <paramref name="purpose"/> isolates the uses: a value protected for one purpose cannot be read with another.
/// </summary>
public interface ISecretProtector
{
    string Protect(string purpose, string plaintext);

    /// <exception cref="System.Security.Cryptography.CryptographicException">The value was not protected for that purpose or its key is gone.</exception>
    string Unprotect(string purpose, string protectedValue);

    byte[] Protect(string purpose, byte[] plaintext);

    byte[] Unprotect(string purpose, byte[] protectedValue);
}
