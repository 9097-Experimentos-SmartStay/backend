using System.Text.RegularExpressions;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public partial record EmployeeCode
{
    public string Value { get; }

    public EmployeeCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Employee code cannot be empty.");

        var trimmed = value.Trim().ToUpperInvariant();
        if (!CodeRegex().IsMatch(trimmed))
            throw new ArgumentException("Employee code must follow format EMP-XXXXX.");

        Value = trimmed;
    }

    [GeneratedRegex(@"^EMP-\d{5}$")]
    private static partial Regex CodeRegex();
}
