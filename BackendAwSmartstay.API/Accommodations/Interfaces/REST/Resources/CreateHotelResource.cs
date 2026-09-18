using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>A hotel to register (US-53 scenario 1). An admin registers only one (D2).</summary>
public record CreateHotelResource
{
    /// <summary>Host of the hotel; only honoured for a chain admin.</summary>
    public int? HostId { get; init; }

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
