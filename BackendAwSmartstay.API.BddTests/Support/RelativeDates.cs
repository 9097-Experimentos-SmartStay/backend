namespace BackendAwSmartstay.API.BddTests.Support;

/// <summary>
/// Resolves the dates written in Gherkin scenarios. Besides ISO dates it understands
/// <c>hoy+N</c> (N days after today) or <c>hoy-N</c> (N days before), so scenarios never go stale as the calendar moves.
/// </summary>
public static class RelativeDates
{
    private const string Today = "hoy";

    public static DateTime Parse(string text)
    {
        var value = text.Trim();
        if (value.StartsWith(Today, StringComparison.OrdinalIgnoreCase) && value.Length > Today.Length
            && value[Today.Length] is '+' or '-')
        {
            var days = int.Parse(value[Today.Length..], System.Globalization.CultureInfo.InvariantCulture);
            return DateTime.UtcNow.Date.AddDays(days);
        }

        return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }
}
