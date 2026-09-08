namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to change the password of an authenticated user.
///     UserId must be extracted from the JWT token, never from the request body.
/// </summary>
/// <param name="UserId">The ID of the authenticated user (from JWT).</param>
/// <param name="CurrentPassword">The user's current raw password for verification.</param>
/// <param name="NewPassword">The new raw password to set.</param>
public record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword);

