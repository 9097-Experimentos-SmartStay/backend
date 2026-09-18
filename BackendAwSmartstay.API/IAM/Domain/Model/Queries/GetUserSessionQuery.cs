namespace BackendAwSmartstay.API.IAM.Domain.Model.Queries;

/// <summary>
///     Asks whether a token issued to <paramref name="UserId"/> with <paramref name="TokenVersion"/> is still valid.
/// </summary>
public record GetUserSessionQuery(int UserId, int TokenVersion);
