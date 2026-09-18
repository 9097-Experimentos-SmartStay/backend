using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using System.Text.RegularExpressions;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public partial record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Phone number cannot be empty.");

        var trimmed = value.Trim();
        if (!PhoneRegex().IsMatch(trimmed))
            throw new DomainValidationException("Invalid phone number format.");

        Value = trimmed;
    }

    [GeneratedRegex(@"^\+?[0-9]{7,15}$")]
    private static partial Regex PhoneRegex();
}
