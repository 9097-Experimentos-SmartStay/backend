using System.ComponentModel.DataAnnotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;

/// <summary>
/// Resource definition for creating a new amenity.
/// </summary>
/// <param name="Name">The name of the amenity (e.g., 'Wifi', 'Pool').</param>
public record CreateAmenityResource([Required] string Name);