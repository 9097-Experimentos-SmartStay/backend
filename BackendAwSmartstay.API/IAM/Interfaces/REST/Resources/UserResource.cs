namespace BackendAwSmartstay.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// Resource representing an enriched user for management views.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Username">Deprecated: same value as <paramref name="Email"/>.</param>
/// <param name="Role">The assigned role hierarchy level.</param>
/// <param name="Status">The active/inactive status.</param>
/// <param name="HotelId">The affiliated hotel, if any.</param>
/// <param name="ChainId">The affiliated chain, if any.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
/// <param name="Email">The account e-mail (login identifier).</param>
public record UserResource(
    int Id, 
    string Username,
    string Role,
    string Status,
    int? HotelId,
    int? ChainId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Email);