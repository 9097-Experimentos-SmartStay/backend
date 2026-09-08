namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to create a new hotel.
/// </summary>
/// <param name="HostId">The identifier of the host creating the hotel.</param>
/// <param name="Name">The name of the hotel.</param>
/// <param name="Address">The street address of the hotel.</param>
/// <param name="City">The city where the hotel is located.</param>
/// <param name="Country">The country where the hotel is located.</param>
/// <param name="ImageUrl">The URL of the hotel's image.</param>
/// <param name="Description">A description of the hotel.</param>
/// <param name="Type">The type of accommodation.</param>
/// <param name="Amenities">The list of amenities provided by the hotel.</param>
public record CreateHotelCommand(
    int HostId,
    string Name,
    string Address,  
    string City,     
    string Country,  
    string ImageUrl,
    string Description,
    string Type,
    List<string> Amenities
);