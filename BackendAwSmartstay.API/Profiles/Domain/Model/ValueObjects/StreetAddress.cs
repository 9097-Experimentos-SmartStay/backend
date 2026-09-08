namespace BackendAwSmartstay.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value object for street address
/// </summary>
/// <param name="Street">
///     The street name
/// </param>
/// <param name="Number">
///     The street number
/// </param>
/// <param name="City">
///     The city name
/// </param>
/// <param name="PostalCode">
///     The postal code
/// </param>
/// <param name="Country">
///     The country name
/// </param>
public record StreetAddress(string Street, string Number, string City, string PostalCode, string Country)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StreetAddress" /> record with empty values.
    /// </summary>
    public StreetAddress() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreetAddress" /> record with the specified street and empty other
    ///     fields.
    /// </summary>
    /// <param name="street">The street name.</param>
    public StreetAddress(string street) : this(street, string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreetAddress" /> record with the specified street, number, and city,
    ///     and empty other fields.
    /// </summary>
    /// <param name="street">The street name.</param>
    /// <param name="number">The street number.</param>
    /// <param name="city">The city name.</param>
    public StreetAddress(string street, string number, string city) : this(street, number, city, string.Empty,
        string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreetAddress" /> record with the specified street, number, city, and
    ///     postal code, and empty country.
    /// </summary>
    /// <param name="street">The street name.</param>
    /// <param name="number">The street number.</param>
    /// <param name="city">The city name.</param>
    /// <param name="postalCode">The postal code.</param>
    public StreetAddress(string street, string number, string city, string postalCode) : this(street, number, city,
        postalCode, string.Empty)
    {
    }

    /// <summary>
    ///     Gets the full address as a formatted string.
    /// </summary>
    public string FullAddress => $"{Street} {Number}, {City}, {PostalCode}, {Country}";
}