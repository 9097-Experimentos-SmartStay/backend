namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>
/// Represents a hotel resource for API responses, containing essential information about a hotel accommodation.
/// </summary>
/// <param name="Id">The unique identifier of the hotel.</param>
/// <param name="HostId">The identifier of the host managing the hotel.</param>
/// <param name="Name">The name of the hotel.</param>
/// <param name="Location">The location of the hotel.</param>
/// <param name="ImageUrl">The URL of the hotel's image.</param>
/// <param name="Description">A description of the hotel.</param>
/// <param name="BasePrice">The base price of the hotel.</param>
/// <param name="Type">The type of accommodation.</param>
/// <param name="Amenities">A list of amenities provided by the hotel.</param>
public record HotelResource(
    int Id,
    int HostId,
    string Name,
    string Location,
    string ImageUrl,
    string Description,
    decimal BasePrice,
    string Type,
    List<string> Amenities
);