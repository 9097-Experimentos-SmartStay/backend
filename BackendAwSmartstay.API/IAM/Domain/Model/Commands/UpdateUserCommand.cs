namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to update an existing user's core attributes.
///     Null values indicate "do not change this field".
/// </summary>
/// <param name="ActorUserId">The ID of the user executing this command (from JWT).</param>
/// <param name="TargetUserId">The ID of the user to update.</param>
/// <param name="NewUsername">Optional new username.</param>
/// <param name="NewPassword">Optional new raw password.</param>
/// <param name="NewHotelId">Optional new hotel affiliation.</param>
/// <param name="NewChainId">Optional new chain affiliation.</param>
public record UpdateUserCommand(
    int ActorUserId,
    int TargetUserId,
    string? NewUsername,
    string? NewPassword,
    int? NewHotelId,
    int? NewChainId);