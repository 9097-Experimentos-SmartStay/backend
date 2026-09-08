using System.Text.RegularExpressions;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
/// Provides extension methods for string manipulation.
/// </summary>
public static partial class StringExtensions
{

    /// <summary>
    /// Converts a string to kebab-case.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The string in kebab-case.</returns>
    public static string ToKebabCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        return KebabCaseRegex().Replace(text, "-$1")
            .Trim()
            .ToLower();
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", RegexOptions.Compiled)]
    private static partial Regex KebabCaseRegex();
}
