using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>The data of a hotel (US-53).</summary>
public record UpdateHotelResource
{
    /// <summary>Hotel name, 2 to 100 characters.</summary>
    /// <example>Hotel Miraflores</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>Street address.</summary>
    /// <example>Av. Pardo 100</example>
    [Required, StringLength(200, MinimumLength = 3)]
    public string Address { get; init; } = string.Empty;

    /// <summary>City.</summary>
    /// <example>Lima</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string City { get; init; } = string.Empty;

    /// <summary>Country.</summary>
    /// <example>Peru</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Country { get; init; } = string.Empty;

    /// <summary>Absolute URL of the main image.</summary>
    /// <example>https://placehold.co/600x400</example>
    [Required, Url, MaxLength(500)]
    public string ImageUrl { get; init; } = string.Empty;

    /// <summary>Description.</summary>
    [Required, MaxLength(1000)]
    public string Description { get; init; } = string.Empty;

    /// <summary>Accommodation type (a category of <c>GET /accommodations/options/categories</c>).</summary>
    /// <example>Hotel</example>
    [Required, StringLength(50, MinimumLength = 2)]
    public string Type { get; init; } = string.Empty;

    /// <summary>Amenities (names of <c>GET /accommodations/options/amenities</c>).</summary>
    public List<string> Amenities { get; init; } = [];
}
