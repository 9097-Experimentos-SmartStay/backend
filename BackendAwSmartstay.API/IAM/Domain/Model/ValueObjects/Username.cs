using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
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
            throw new DomainValidationException("Username cannot be empty or whitespace.");

        if (value.Length > 100)
            throw new DomainValidationException("Username cannot exceed 100 characters.");

        var normalized = value.ToLowerInvariant();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^[a-z0-9_.@]+$"))
            throw new DomainValidationException(
                "Username must be lowercase alphanumeric and may contain '.', '_', or '@'.");

        Value = normalized;
    }

    public static implicit operator string(Username username) => username.Value;
    public static implicit operator Username(string value) => new(value);

    public override string ToString() => Value;
}