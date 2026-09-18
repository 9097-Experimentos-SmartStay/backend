using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST.Authorization;

/// <summary>The user account a guest profile belongs to (or is about to be linked to); null when unlinked.</summary>
public sealed record GuestAccount(int? UserId);

/// <summary>Requirement "the requester may act on the guest profile of this account".</summary>
public sealed class GuestAccountRequirement : IAuthorizationRequirement
{
    public static readonly GuestAccountRequirement Instance = new();

    private GuestAccountRequirement() { }
}

/// <summary>
///     Guest profile scope: desk staff and administrators (already filtered by the policies) act on any profile;
///     a guest only on the profile of their own account.
/// </summary>
public sealed class GuestAccountAuthorizationHandler : AuthorizationHandler<GuestAccountRequirement, GuestAccount>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, GuestAccountRequirement requirement, GuestAccount account)
    {
        if (!context.User.IsGuest() || (account.UserId is { } userId && userId == context.User.GetUserId()))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
