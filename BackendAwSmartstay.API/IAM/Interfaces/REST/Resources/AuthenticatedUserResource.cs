namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource representing a successfully authenticated user response.
/// </summary>
/// <param name="Id">The user's unique identifier.</param>
/// <param name="Username">The user's username.</param>
/// <param name="Token">The JWT access token.</param>
/// <param name="Role">The user's assigned role.</param>
/// <param name="HotelId">The user's affiliated hotel ID, if any.</param>
/// <param name="ChainId">The user's affiliated chain ID, if any.</param>
public record AuthenticatedUserResource(
    int Id, 
    string Username, 
    string Token, 
    string Role, 
    int? HotelId, 
    int? ChainId);