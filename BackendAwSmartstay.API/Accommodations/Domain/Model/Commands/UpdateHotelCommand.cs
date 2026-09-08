namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to update an existing hotel aggregate.
/// </summary>
/// <param name="Id">The unique identifier of the hotel to update.</param>
/// <param name="Name">The new name.</param>
/// <param name="Address">The new address.</param>
/// <param name="City">The new city.</param>
/// <param name="Country">The new country.</param>
/// <param name="ImageUrl">The new image URL.</param>
/// <param name="Description">The new description.</param>
/// <param name="Type">The new type.</param>
/// <param name="Amenities">The new list of amenities.</param>
public record UpdateHotelCommand(
    int Id,
    string Name,
    string Address,
    string City,
    string Country,
    string ImageUrl,
    string Description,
    string Type,
    List<string> Amenities
);