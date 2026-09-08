using Humanizer;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Provides extension methods for string manipulation, such as naming convention conversions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to snake_case.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The converted snake_case string.</returns>
    public static string ToSnakeCase(this string text)
    {
        return new string(Convert(text.GetEnumerator()).ToArray());

        static IEnumerable<char> Convert(CharEnumerator e)
        {
            if (!e.MoveNext()) yield break;

            yield return char.ToLower(e.Current);

            while (e.MoveNext())
                if (char.IsUpper(e.Current))
                {
                    yield return '_';
                    yield return char.ToLower(e.Current);
                }
                else
                {
                    yield return e.Current;
                }
        }
    }

    /// <summary>
    ///     Convert the string to plural
    /// </summary>
    /// <param name="text">
    ///     The text to convert to plural
    /// </param>
    /// <returns>
    ///     The pluralized text
    /// </returns>
    public static string ToPlural(this string text)
    {
        return text.Pluralize(false);
    }
}

