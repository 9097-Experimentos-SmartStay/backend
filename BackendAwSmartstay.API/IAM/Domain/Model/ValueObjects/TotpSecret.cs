using System.Security.Cryptography;
using System.Text;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Shared secret of a TOTP authenticator (US-52): 160 random bits (the HMAC-SHA1 key size recommended by
///     RFC 4226), shown to the user once as Base32 (RFC 4648, no padding) inside the QR code.
/// </summary>
public sealed class TotpSecret
{
    public const int KeyLength = 20;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    private readonly byte[] _key;

    private TotpSecret(byte[] key)
    {
        if (key.Length < 10)
            throw new DomainValidationException("A TOTP secret needs at least 80 bits.");
        _key = key;
    }

    /// <summary>A new random secret.</summary>
    public static TotpSecret Generate() => new(RandomNumberGenerator.GetBytes(KeyLength));

    /// <summary>Rebuilds a stored secret from its Base32 text.</summary>
    public static TotpSecret FromBase32(string base32)
    {
        var clean = base32.Trim().TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>(clean.Length * 5 / 8);
        int buffer = 0, bits = 0;
        foreach (var c in clean)
        {
            var value = Base32Alphabet.IndexOf(c);
            if (value < 0) throw new DomainValidationException("The TOTP secret is not valid Base32.");
            buffer = (buffer << 5) | value;
            bits += 5;
            if (bits < 8) continue;
            bits -= 8;
            bytes.Add((byte)(buffer >> bits));
            buffer &= (1 << bits) - 1;
        }
        return new TotpSecret(bytes.ToArray());
    }

    /// <summary>The secret as Base32 without padding (what authenticator apps expect).</summary>
    public string ToBase32()
    {
        var text = new StringBuilder((_key.Length * 8 + 4) / 5);
        int buffer = 0, bits = 0;
        foreach (var b in _key)
        {
            buffer = (buffer << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                text.Append(Base32Alphabet[(buffer >> bits) & 31]);
            }
            buffer &= (1 << bits) - 1;
        }
        if (bits > 0) text.Append(Base32Alphabet[(buffer << (5 - bits)) & 31]);
        return text.ToString();
    }

    /// <summary>HMAC-SHA1 of <paramref name="message"/> with the secret (RFC 4226 step 1).</summary>
    internal byte[] Sign(ReadOnlySpan<byte> message) => HMACSHA1.HashData(_key, message);
}
