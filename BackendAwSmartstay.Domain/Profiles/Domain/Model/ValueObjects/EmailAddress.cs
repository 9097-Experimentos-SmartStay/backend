using System.Text.RegularExpressions;

namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public partial record EmailAddress
{
    public string Address { get; }

    public EmailAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email address cannot be empty.");

        var trimmed = address.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(trimmed))
            throw new ArgumentException("Invalid email address format.");

        Address = trimmed;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}