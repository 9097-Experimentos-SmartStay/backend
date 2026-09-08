namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to create a new user with full control over role, hotel and chain assignment.
///     The ActorUserId identifies the user performing the creation for authorization checks.
/// </summary>
/// <param name="ActorUserId">The ID of the user executing this command (from JWT).</param>
/// <param name="Username">The desired username.</param>
/// <param name="Password">The raw password.</param>
/// <param name="Role">The role to assign (must be lower in hierarchy than the actor's).</param>
/// <param name="HotelId">Optional hotel affiliation.</param>
/// <param name="ChainId">Optional chain affiliation.</param>
public record CreateUserCommand(
    int ActorUserId,
    string Username,
    string Password,
    string Role,
    int? HotelId,
    int? ChainId);