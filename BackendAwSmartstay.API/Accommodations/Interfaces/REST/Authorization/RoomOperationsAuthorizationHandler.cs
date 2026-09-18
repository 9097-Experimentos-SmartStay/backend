using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST.Authorization;

/// <summary>Requirement "the requester operates this room" (e.g. changes its status).</summary>
public sealed class RoomOperationsRequirement : IAuthorizationRequirement
{
    public static readonly RoomOperationsRequirement Instance = new();

    private RoomOperationsRequirement() { }
}

/// <summary>
///     Hotel staff (reception, housekeeping, maintenance, admin) operate the rooms of the hotel they are assigned
///     to; a chain administrator operates every room. Which roles may call the endpoint is decided by
///     <see cref="Policies.UpdateRoomStatus"/>.
/// </summary>
public sealed class RoomOperationsAuthorizationHandler : AuthorizationHandler<RoomOperationsRequirement, Room>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoomOperationsRequirement requirement, Room room)
    {
        var user = context.User;
        if (user.IsChainAdmin() || (user.GetHotelId() is { } hotelId && hotelId == room.HotelId))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
