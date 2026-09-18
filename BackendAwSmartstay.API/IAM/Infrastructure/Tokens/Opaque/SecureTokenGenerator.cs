using System.Security.Cryptography;
using System.Text;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using Microsoft.AspNetCore.WebUtilities;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Tokens.Opaque;

/// <summary>
///     256-bit random tokens (base64url, safe in links) stored as SHA-256 hashes. A fast hash is enough: the input
///     is random and long, so it cannot be brute-forced like a password.
/// </summary>
public class SecureTokenGenerator : ISecureTokenGenerator
{
    private const int TokenBytes = 32;

    public SecureToken Generate()
    {
        var value = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));
        return new SecureToken(value, Hash(value));
    }

    public string Hash(string tokenValue) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(tokenValue ?? string.Empty)));
}
