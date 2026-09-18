using System.Security.Cryptography;
using System.Text;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     One-time recovery codes (US-52 scenario 3): 10 characters shown as <c>XXXXX-XXXXX</c> from an alphabet
///     without look-alike characters (no 0/O, 1/I/L). Case, spaces and hyphens are ignored when typed.
/// </summary>
public static class RecoveryCodeFormat
{
    public const int CodesPerEnrollment = 10;
    private const int Length = 10;
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    /// <summary>A new random code, formatted for display.</summary>
    public static string NewCode()
    {
        var chars = new char[Length];
        for (var i = 0; i < Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return $"{new string(chars, 0, 5)}-{new string(chars, 5, 5)}";
    }

    /// <summary>The canonical form that is hashed and compared (upper case, no separators).</summary>
    public static string Canonicalize(string code)
    {
        var text = new StringBuilder(Length);
        foreach (var c in code)
            if (char.IsLetterOrDigit(c)) text.Append(char.ToUpperInvariant(c));
        return text.ToString();
    }

    /// <summary>True when <paramref name="code"/> has the shape of a recovery code.</summary>
    public static bool IsWellFormed(string? code) =>
        code is not null && Canonicalize(code) is { Length: Length } canonical && canonical.All(Alphabet.Contains);
}
