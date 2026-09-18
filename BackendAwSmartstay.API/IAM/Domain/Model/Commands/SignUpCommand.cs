namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to register a new user (US-01).
/// </summary>
/// <param name="FirstName">First name of the owner.</param>
/// <param name="LastName">Last name of the owner.</param>
/// <param name="Email">Login e-mail.</param>
/// <param name="Password">The raw password.</param>
/// <param name="Role">Optional role to assign (requires administrative actor).</param>
/// <param name="ActorUserId">Optional ID of the user executing this command (from JWT).</param>
public record SignUpCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Role = null,
    int? ActorUserId = null);
