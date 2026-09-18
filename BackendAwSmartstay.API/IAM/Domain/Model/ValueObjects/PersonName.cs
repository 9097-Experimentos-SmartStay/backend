using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     First and last name of the person who owns an account (US-01 required registration data). Letters of any
///     language (accents included) separated by single spaces, hyphens or apostrophes; 2 to 50 characters each.
/// </summary>
public sealed partial record PersonName
{
    public const int MinLength = 2;
    public const int MaxLength = 50;

    public PersonName(string firstName, string lastName)
    {
        FirstName = Normalize(firstName, "First name");
        LastName = Normalize(lastName, "Last name");
    }

    public string FirstName { get; }
    public string LastName { get; }

    public string FullName => $"{FirstName} {LastName}";

    /// <summary>True when <paramref name="value"/> is a valid name part (used by request validation too).</summary>
    public static bool IsValidPart(string? value)
    {
        var collapsed = Collapse(value);
        return collapsed.Length is >= MinLength and <= MaxLength && NamePattern().IsMatch(collapsed);
    }

    private static string Normalize(string value, string label)
    {
        var collapsed = Collapse(value);
        if (collapsed.Length == 0)
            throw new DomainValidationException($"{label} is required.");
        if (collapsed.Length is < MinLength or > MaxLength)
            throw new DomainValidationException($"{label} must have between {MinLength} and {MaxLength} characters.");
        if (!NamePattern().IsMatch(collapsed))
            throw new DomainValidationException($"{label} can only contain letters, spaces, hyphens and apostrophes.");
        return collapsed;
    }

    private static string Collapse(string? value) => WhiteSpace().Replace(value?.Trim() ?? string.Empty, " ");

    [GeneratedRegex(@"^[\p{L}\p{M}]+(?:[ '’-][\p{L}\p{M}]+)*$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpace();
}
