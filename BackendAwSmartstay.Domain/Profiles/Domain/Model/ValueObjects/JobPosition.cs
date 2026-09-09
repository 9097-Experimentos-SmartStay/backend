namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public record JobPosition
{
    public string Value { get; }

    public JobPosition(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Job position cannot be empty.");

        var trimmed = value.Trim();
        if (trimmed.Length is < 3 or > 100)
            throw new ArgumentException("Job position must be between 3 and 100 characters.");

        Value = trimmed;
    }
}
