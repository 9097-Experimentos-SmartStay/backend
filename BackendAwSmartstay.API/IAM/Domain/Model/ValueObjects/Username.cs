namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
/// Value Object that encapsulates username validation and normalization rules.
/// </summary>
public sealed record Username
{
    public string Value { get; }

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Username cannot exceed 100 characters.", nameof(value));

        var normalized = value.ToLowerInvariant();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^[a-z0-9_.@]+$"))
            throw new ArgumentException(
                "Username must be lowercase alphanumeric and may contain '.', '_', or '@'.",
                nameof(value));

        Value = normalized;
    }

    public static implicit operator string(Username username) => username.Value;
    public static implicit operator Username(string value) => new(value);

    public override string ToString() => Value;
}