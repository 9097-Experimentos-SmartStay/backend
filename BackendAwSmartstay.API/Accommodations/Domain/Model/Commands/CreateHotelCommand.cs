using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;

namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

/// <summary>
/// Command to create a new hotel.
/// </summary>
/// <param name="Registrant">The administrator who registers the hotel.</param>
/// <param name="RequestedHostId">Host requested in the request (only honoured for chain administrators).</param>
/// <param name="Name">The name of the hotel.</param>
/// <param name="Address">The street address of the hotel.</param>
/// <param name="City">The city where the hotel is located.</param>
/// <param name="Country">The country where the hotel is located.</param>
/// <param name="ImageUrl">The URL of the hotel's image.</param>
/// <param name="Description">A description of the hotel.</param>
/// <param name="Type">The type of accommodation.</param>
/// <param name="Amenities">The list of amenities provided by the hotel.</param>
/// <param name="RegistrantSession">
///     The registrant's current session (IAM published language): a hotel administrator gets a new session with
///     their hotel, renewing this one.
/// </param>
public record CreateHotelCommand(
    HotelRegistrant Registrant,
    int? RequestedHostId,
    string Name,
    string Address,  
    string City,     
    string Country,  
    string ImageUrl,
    string Description,
    string Type,
    List<string> Amenities,
    SessionContext RegistrantSession
);