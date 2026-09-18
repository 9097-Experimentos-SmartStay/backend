using BackendAwSmartstay.API.IAM.Domain.Model.Enums;

namespace BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Current authorization data of a user, as seen when one of their access tokens is presented.
///     Role and scope come from the <c>User</c> aggregate (the single source of truth for authorization),
///     so changes made by an administrator apply on the next request (US-03: "access rights are modified
///     immediately") without waiting for the token to expire.
/// </summary>
public sealed record UserSession(UserSessionStatus Status, string? Role = null, int? HotelId = null, int? ChainId = null)
{
    public bool IsValid => Status == UserSessionStatus.Valid;

    public static UserSession NotFound() => new(UserSessionStatus.UserNotFound);
}
