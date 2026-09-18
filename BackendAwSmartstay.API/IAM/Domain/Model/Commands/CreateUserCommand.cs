namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to create a user from the administration (US-03 scenario 1: staff users with a role).
///     The ActorUserId identifies the administrator for authorization checks.
/// </summary>
/// <param name="ActorUserId">The ID of the user executing this command (from JWT).</param>
/// <param name="FirstName">First name of the new user.</param>
/// <param name="LastName">Last name of the new user.</param>
/// <param name="Email">Login e-mail.</param>
/// <param name="Password">The initial raw password.</param>
/// <param name="Role">The role to assign (must be lower in hierarchy than the actor's).</param>
/// <param name="HotelId">Hotel of the new user (defaults to the hotel of a hotel administrator).</param>
/// <param name="ChainId">Optional chain affiliation.</param>
public record CreateUserCommand(
    int ActorUserId,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    int? HotelId,
    int? ChainId);
