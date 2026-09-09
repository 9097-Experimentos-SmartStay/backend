namespace BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

/// <summary>
/// Anti-Corruption Layer (ACL) facade for the Accommodations Bounded Context.
/// Exposes minimum required query operations for cross-context consumption without leaking domain entities.
/// </summary>
public interface IAccommodationsContextFacade
{
    /// <summary>
    /// Checks whether a hotel exists in the Accommodations Bounded Context.
    /// </summary>
    /// <param name="hotelId">The canonical hotel identifier (int).</param>
    /// <returns>True if the hotel exists; otherwise, false.</returns>
    Task<bool> HotelExistsAsync(int hotelId);
}
