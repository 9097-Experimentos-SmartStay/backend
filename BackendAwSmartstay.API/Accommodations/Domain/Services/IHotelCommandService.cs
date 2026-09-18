using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

/// <summary>
///     Result of registering a hotel (US-53 scenario 1).
/// </summary>
/// <param name="Hotel">The new hotel.</param>
/// <param name="RegistrantSession">
///     New credentials of a hotel administrator who registered their own hotel (it became their hotel and their
///     previous tokens were revoked); null for a chain administrator.
/// </param>
public sealed record HotelRegistration(Hotel Hotel, IAM.Interfaces.ACL.ReissuedSession? RegistrantSession);

/// <summary>
/// Defines the contract for services that handle hotel state changes (Create, Update, Delete).
/// </summary>
public interface IHotelCommandService
{
    /// <summary>
    /// Handles the creation of a hotel.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <returns>The created hotel and, for a hotel administrator registering their own hotel, their new session.</returns>
    Task<HotelRegistration> Handle(CreateHotelCommand command);
    
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

    /// <summary>US-53: sets the payment methods of a hotel.</summary>
    /// <returns>The hotel, or null when it does not exist.</returns>
    /// <exception cref="BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions.InvalidFieldsException">Invalid settings (every violation).</exception>
    Task<Hotel?> Handle(ConfigureHotelPaymentSettingsCommand command);
}