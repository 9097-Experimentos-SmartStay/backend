namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to register a new user.
/// </summary>
/// <param name="Username">The desired username.</param>
/// <param name="Password">The raw password.</param>
/// <param name="Role">Optional role to assign (requires administrative actor).</param>
/// <param name="ActorUserId">Optional ID of the user executing this command (from JWT).</param>
public record SignUpCommand(
    string Username, 
    string Password, 
    string? Role = null, 
    int? ActorUserId = null);