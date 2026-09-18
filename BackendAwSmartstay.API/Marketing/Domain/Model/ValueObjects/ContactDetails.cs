using BackendAwSmartstay.API.Marketing.Domain.Model.Exceptions;
using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;

/// <summary>
///     Contact of the visitor who asks for a demo. Mirrors the landing form (US-27): names with letters of any
///     language (accents included) 2–50, e-mail <c>name@domain.tld</c> up to 254, optional phone with an optional
///     leading <c>+</c> and 7–15 digits (spaces, dots, dashes and parentheses are ignored).
/// </summary>
public sealed partial record ContactDetails
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int EmailMaxLength = 254;

    public ContactDetails(string firstName, string lastName, string email, string? phone)
    {
        FirstName = Name(firstName, "First name");
        LastName = Name(lastName, "Last name");

        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!IsValidEmail(normalizedEmail))
            throw new DomainValidationException(MarketingErrorCodes.EmailInvalid, "Enter a valid e-mail address (for example name@domain.com).");
        Email = normalizedEmail;

        var normalizedPhone = NormalizePhone(phone);
        if (normalizedPhone is not null && !PhonePattern().IsMatch(normalizedPhone))
            throw new DomainValidationException(MarketingErrorCodes.PhoneInvalid, "The phone must have 7 to 15 digits and may start with +.");
        Phone = normalizedPhone;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Phone { get; }

    public string FullName => $"{FirstName} {LastName}";

    public static bool IsValidName(string? value)
    {
        var collapsed = Collapse(value);
        return collapsed.Length is >= NameMinLength and <= NameMaxLength && NamePattern().IsMatch(collapsed);
    }

    public static bool IsValidEmail(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length is > 0 and <= EmailMaxLength && EmailPattern().IsMatch(normalized);
    }

    public static bool IsValidPhone(string? value)
    {
        var normalized = NormalizePhone(value);
        return normalized is null || PhonePattern().IsMatch(normalized);
    }

    /// <summary>Keeps "+" and digits: "+51 (987) 654-321" → "+51987654321". Blank → null.</summary>
    public static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return PhoneSeparators().Replace(value.Trim(), string.Empty);
    }

    private static string Name(string value, string label)
    {
        var collapsed = Collapse(value);
        if (collapsed.Length == 0) throw new DomainValidationException(MarketingErrorCodes.FieldRequired, $"{label} is required.");
        if (!IsValidName(collapsed))
            throw new DomainValidationException(MarketingErrorCodes.NameInvalid, $"{label} must have 2 to 50 letters (spaces, hyphens and apostrophes allowed).");
        return collapsed;
    }

    private static string Collapse(string? value) => WhiteSpace().Replace(value?.Trim() ?? string.Empty, " ");

    [GeneratedRegex(@"^[\p{L}\p{M}]+(?:[ '’-][\p{L}\p{M}]+)*$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^[^\s@]+@[^\s@.]+(?:\.[^\s@.]+)*\.[^\s@.]{2,}$")]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhonePattern();

    [GeneratedRegex(@"[\s().-]")]
    private static partial Regex PhoneSeparators();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpace();
}
