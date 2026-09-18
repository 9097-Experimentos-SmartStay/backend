using System.Text.RegularExpressions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

/// <summary>Rules of the room number (US-53): 1 to 10 letters, digits or hyphens, stored in upper case.</summary>
public static partial class RoomNumber
{
    public const int MaxLength = 10;
    public const decimal MaxPrice = 100_000m;

    public static string Normalize(string? number)
    {
        var normalized = number?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!Format().IsMatch(normalized))
            throw new InvalidFieldException("number", "The room number must have 1 to 10 letters, digits or hyphens (e.g. 101 or 2B).");
        return normalized;
    }

    [GeneratedRegex(@"^[A-Z0-9][A-Z0-9-]{0,9}$")]
    private static partial Regex Format();
}
