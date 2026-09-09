namespace BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;

public record StreetAddress
{
    public string Street { get; }
    public string Number { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public StreetAddress(string street, string number, string city, string postalCode, string country)
    {
        Street = street?.Trim() ?? string.Empty;
        Number = number?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        PostalCode = postalCode?.Trim() ?? string.Empty;
        Country = country?.Trim() ?? string.Empty;
    }

    public string FullAddress => $"{Street} {Number}, {City}, {PostalCode}, {Country}";
}