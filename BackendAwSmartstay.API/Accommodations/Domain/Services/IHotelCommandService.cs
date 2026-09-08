using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

/// <summary>
/// Defines the contract for services that handle hotel state changes (Create, Update, Delete).
/// </summary>
public interface IHotelCommandService
{
    /// <summary>
    /// Handles the creation of a hotel.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <returns>The created hotel or null if creation failed.</returns>
    Task<Hotel?> Handle(CreateHotelCommand command);
    
    /// <summary>
    /// Handles the update of a hotel.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <returns>The updated hotel or null if not found.</returns>
    Task<Hotel?> Handle(UpdateHotelCommand command);

    /// <summary>
    /// Handles the deletion of a hotel.
    /// </summary>
    /// <param name="command">The delete command.</param>
    /// <returns>The deleted hotel or null if not found.</returns>
    Task<Hotel?> Handle(DeleteHotelCommand command);
}