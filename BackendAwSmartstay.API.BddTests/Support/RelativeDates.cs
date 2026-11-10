namespace BackendAwSmartstay.API.BddTests.Support;

/// <summary>Parses ISO dates or <c>hoy+N</c> / <c>hoy-N</c> (days from today) so scenarios never go stale.</summary>
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
