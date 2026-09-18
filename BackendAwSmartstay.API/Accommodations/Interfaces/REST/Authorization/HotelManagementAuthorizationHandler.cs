using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Authorization;

/// <summary>
///     Requirement "the requester may manage this hotel (and its rooms)". Evaluated against a concrete
///     <see cref="Hotel"/> with <see cref="IAuthorizationService"/> (resource-based authorization).
/// </summary>
public sealed class HotelManagementRequirement : IAuthorizationRequirement
{
    public static readonly HotelManagementRequirement Instance = new();

    private HotelManagementRequirement() { }
}

/// <summary>
///     Hotel scope (US-42: each hotel is configured "without affecting the others"):
///     <list type="bullet">
///         <item>chain_admin: every hotel (chains are not modelled on Hotel yet);</item>
///         <item>admin: only the hotel assigned to them in IAM (<c>hotel_id</c>);</item>
///         <item>anyone else: no hotel (the <see cref="Policies.ManageHotels"/> policy already rejects them).</item>
///     </list>
/// </summary>
public sealed class HotelManagementAuthorizationHandler
    : AuthorizationHandler<HotelManagementRequirement, Hotel>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, HotelManagementRequirement requirement, Hotel hotel)
    {
        var user = context.User;
        if (user.IsChainAdmin() || (user.IsHotelAdmin() && user.GetHotelId() == hotel.Id))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
