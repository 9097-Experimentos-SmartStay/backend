using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Authorization;

/// <summary>Requirement "the requester works at this hotel" (e.g. reads its payment methods).</summary>
public sealed class HotelStaffRequirement : IAuthorizationRequirement
{
    public static readonly HotelStaffRequirement Instance = new();

    private HotelStaffRequirement() { }
}

/// <summary>
///     The staff of a hotel (the roles allowed by the endpoint's policy) only see their own hotel (<c>hotel_id</c>);
///     a chain administrator sees every hotel.
/// </summary>
public sealed class HotelStaffAuthorizationHandler : AuthorizationHandler<HotelStaffRequirement, Hotel>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, HotelStaffRequirement requirement, Hotel hotel)
    {
        var user = context.User;
        if (user.IsChainAdmin() || (user.GetHotelId() is { } hotelId && hotelId == hotel.Id))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
