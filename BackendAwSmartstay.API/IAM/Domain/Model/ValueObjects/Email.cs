using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     E-mail address that identifies an account (US-01, US-02, US-04: users register and sign in with their
///     e-mail). Normalized to lower case so the identifier is unique regardless of casing.
/// </summary>
public sealed partial record Email
{
    public const int MaxLength = 254;

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException(IamErrorCodes.EmailRequired, "Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainValidationException(IamErrorCodes.EmailTooLong, $"Email cannot exceed {MaxLength} characters.");

        if (!EmailFormat().IsMatch(normalized))
            throw new DomainValidationException(IamErrorCodes.EmailInvalid, "Email has an invalid format.");

        Value = normalized;
    }

    private Email(string value, bool _) => Value = value;

    /// <summary>True when <paramref name="value"/> is a valid login e-mail (used by request validation).</summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.Trim();
        return normalized.Length <= MaxLength && EmailFormat().IsMatch(normalized);
    }

    /// <summary>
    ///     Rebuilds a stored identifier without re-validating it. Accounts created before e-mail became the login
    ///     identifier may hold a plain username; they must still load (e.g. to be listed or updated).
    /// </summary>
    internal static Email FromPersistence(string value) => new(value, false);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormat();
}
