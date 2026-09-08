using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for updating an existing hotel.
/// Represents the data payload received from the client.
/// </summary>
public record UpdateHotelResource(
    [Required] string Name,
    [Required] string Address,
    [Required] string City,
    [Required] string Country,
    [Required] string ImageUrl,
    [Required] string Description,
    [Required] string Type,
    List<string> Amenities
);