using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for creating a new hotel.
/// </summary>
public record CreateHotelResource(
    [Required] int HostId,
    [Required] string Name,
    [Required] string Address, 
    [Required] string City,     
    [Required] string Country, 
    [Required] string ImageUrl,
    [Required] string Description,
    [Required] string Type,
    List<string> Amenities
);