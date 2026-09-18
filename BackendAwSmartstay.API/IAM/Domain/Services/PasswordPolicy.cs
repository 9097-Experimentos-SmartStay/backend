using System.Globalization;
using System.Text;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>Outcome of checking a new password against <see cref="PasswordPolicy"/>.</summary>
/// <param name="Code">Stable code of the broken rule (<c>password.*</c>), or null when the password is acceptable.</param>
/// <param name="Problem">Why the password is rejected, or null when it is acceptable.</param>
/// <param name="Parameters">Values of the broken rule (e.g. <c>minLength</c>), or null.</param>
public sealed record PasswordCheck(string? Code, string? Problem, IReadOnlyDictionary<string, object?>? Parameters = null)
{
    public static readonly PasswordCheck Acceptable = new(null, null);

    public bool IsAcceptable => Code is null;

    internal static PasswordCheck Rejected(string code, string problem, IReadOnlyDictionary<string, object?>? parameters = null) =>
        new(code, problem, parameters);
}

/// <summary>
///     Password rules of NIST SP 800-63B-4 (§3.1.1.2), the team's security policy for every account:
///     <list type="bullet">
///         <item>Length is the only strength rule: at least 8 characters for staff accounts, which also use a second
///         factor (MFA TOTP), and at least 15 for guests, whose password is their only factor. At most 128.</item>
///         <item>Any character is allowed (spaces, emoji, accents): the text is normalized to Unicode NFKC and every
///         code point counts as one character.</item>
///         <item>No composition rules (no "one upper case, one symbol...") and no periodic expiry.</item>
///         <item>A blocklist rejects commonly used passwords, repetitive or sequential runs and context-specific
///         words (the account e-mail, the service name). Passwords found in breaches are rejected by the
///         application layer through the breached password port.</item>
///     </list>
///     The minimum applies when a password is set: an account whose role changes between guest and staff keeps
///     its current password and meets the new minimum the next time a password is set.
/// </summary>
public static class PasswordPolicy
{
    public const int StaffMinimumLength = 8;
    public const int GuestMinimumLength = 15;
    public const int MaximumLength = 128;

    public const string BreachedPasswordProblem =
        "This password has appeared in a known data breach. Choose a different one.";

    /// <summary>The minimum length for an account with <paramref name="role"/>.</summary>
    public static int MinimumLengthFor(Role role) =>
        role.RequiresMultiFactorAuthentication ? StaffMinimumLength : GuestMinimumLength;

    /// <summary>NFKC normalization applied before a password is checked, hashed or verified.</summary>
    public static string Normalize(string password) => password.Normalize(NormalizationForm.FormKC);

    /// <summary>Checks <paramref name="password"/> for an account with <paramref name="role"/> and <paramref name="email"/>.</summary>
    public static PasswordCheck Check(string? password, Role role, Email? email)
    {
        if (string.IsNullOrEmpty(password))
            return PasswordCheck.Rejected(IamErrorCodes.PasswordRequired, "Enter a password.");

        var normalized = Normalize(password);
        var length = normalized.EnumerateRunes().Count();
        var minimum = MinimumLengthFor(role);

        if (length < minimum)
            return PasswordCheck.Rejected(IamErrorCodes.PasswordTooShort,
                role.RequiresMultiFactorAuthentication
                    ? $"Use at least {minimum} characters."
                    : $"Use at least {minimum} characters. A passphrase of a few words is long and easy to remember.",
                new Dictionary<string, object?> { ["minLength"] = minimum });
        if (length > MaximumLength)
            return PasswordCheck.Rejected(IamErrorCodes.PasswordTooLong,
                $"The password cannot exceed {MaximumLength} characters.",
                new Dictionary<string, object?> { ["maxLength"] = MaximumLength });

        var folded = normalized.ToLower(CultureInfo.InvariantCulture);
        if (CommonPasswords.Contains(folded) || ContainsServiceName(folded))
            return PasswordCheck.Rejected(IamErrorCodes.PasswordTooCommon, "This password is too common or easy to guess. Choose a different one.");
        if (IsRepetitiveOrSequential(folded))
            return PasswordCheck.Rejected(IamErrorCodes.PasswordRepetitive, "The password cannot be a repeated or sequential run of characters (such as 'aaaaaaaa' or '12345678').");
        if (email is not null && IsDerivedFromEmail(folded, email))
            return PasswordCheck.Rejected(IamErrorCodes.PasswordContainsEmail, "The password cannot be your e-mail address or its user name.");

        return PasswordCheck.Acceptable;
    }

    private static bool ContainsServiceName(string folded)
    {
        // Context-specific words: the service name alone, or padded with digits and symbols.
        var letters = new string(folded.Where(char.IsLetter).ToArray());
        return letters is "smartstay" or "smartstayhotel" or "smartstayadmin";
    }

    private static bool IsDerivedFromEmail(string folded, Email email)
    {
        var address = email.Value;
        var localPart = address.Split('@')[0];
        var compact = new string(folded.Where(char.IsLetterOrDigit).ToArray());
        return folded == address || folded == localPart
               || compact == new string(localPart.Where(char.IsLetterOrDigit).ToArray());
    }

    private static bool IsRepetitiveOrSequential(string folded)
    {
        var runes = folded.EnumerateRunes().Select(rune => rune.Value).ToArray();
        if (runes.Length < 2) return false;
        if (runes.All(value => value == runes[0])) return true;

        var ascending = true;
        var descending = true;
        for (var i = 1; i < runes.Length; i++)
        {
            ascending &= runes[i] == runes[i - 1] + 1;
            descending &= runes[i] == runes[i - 1] - 1;
        }
        if (ascending || descending) return true;

        // A short block repeated to fill the length ("abcabcabc", "12121212").
        for (var block = 2; block <= runes.Length / 2; block++)
        {
            if (runes.Length % block != 0) continue;
            var repeated = true;
            for (var i = block; i < runes.Length && repeated; i++)
                repeated = runes[i] == runes[i % block];
            if (repeated) return true;
        }
        return false;
    }

    /// <summary>
    ///     Small embedded blocklist: the most used passwords of public leak rankings (English and Spanish) that the
    ///     length rule alone would accept. The breached password check covers the long tail.
    /// </summary>
    private static readonly HashSet<string> CommonPasswords = new(StringComparer.Ordinal)
    {
        "password", "password1", "password12", "password123", "password1234", "password!", "password!!",
        "passw0rd", "p@ssw0rd", "p@ssword", "p@ssword1", "passw0rd1", "passwordpassword", "mypassword",
        "12345678", "123456789", "1234567890", "12345678910", "123456789012345", "1234567890123456",
        "87654321", "987654321", "0987654321", "11111111", "00000000", "88888888", "12341234", "123123123",
        "11223344", "112233445566", "123321123", "1q2w3e4r", "1q2w3e4r5t", "q1w2e3r4", "q1w2e3r4t5",
        "qwertyui", "qwertyuiop", "qwertyuiopasdfg", "qwerty123", "qwerty1234", "qwerty12345", "1234qwer",
        "qwer1234", "asdfghjk", "asdfghjkl", "zxcvbnm1", "zxcvbnmasdfghjkl", "qazwsxedc", "1qaz2wsx",
        "zaq12wsx", "1qazxsw2", "abcd1234", "abc12345", "abc123456", "aa123456", "a1234567", "a12345678",
        "iloveyou", "iloveyou1", "iloveyouiloveyou", "sunshine", "sunshine1", "princess", "princess1",
        "football", "football1", "baseball", "superman", "batman123", "starwars", "trustno1", "whatever",
        "welcome1", "welcome123", "welcometosmartstay", "letmein1", "letmein123", "changeme", "changeme123",
        "admin123", "admin1234", "administrator", "administrador", "master123", "dragon123", "monkey123",
        "shadow123", "michael1", "computer", "internet", "chocolate", "pokemon123", "minecraft",
        "contraseña", "contrasena", "contraseña1", "contraseña123", "contrasena123", "micontraseña",
        "contraseñacontraseña", "clave123", "clave1234", "miclave123", "teamo123", "teamomucho", "tequiero",
        "tequiero123", "bienvenido", "bienvenido1", "bienvenido123", "hotel123", "hotel1234", "recepcion",
        "recepcion123", "123456789a", "12345678a", "peru1234", "lima1234", "futbol123", "alianzalima",
        "universitario", "estrella", "mariposa", "corazon", "princesa", "america1", "colombia", "argentina",
        "mexico123", "barcelona", "realmadrid", "correcthorsebatterystaple"
    };
}
