namespace BackendAwSmartstay.API.BddTests.Support;

/// <summary>
/// Resolves the dates written in Gherkin scenarios. Besides ISO dates it understands
/// <c>hoy+N</c> (N days after today), so scenarios never go stale as the calendar moves.
/// </summary>
public static class RelativeDates
{
    private const string Prefix = "hoy+";

    public static DateTime Parse(string text)
    {
        var value = text.Trim();
        if (value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var days = int.Parse(value[Prefix.Length..], System.Globalization.CultureInfo.InvariantCulture);
            return DateTime.UtcNow.Date.AddDays(days);
        }

        return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }
}
