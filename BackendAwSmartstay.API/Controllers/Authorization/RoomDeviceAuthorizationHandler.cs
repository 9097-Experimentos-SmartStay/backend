using BackendAwSmartstay.API.Bookings.Interfaces.ACL;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.Controllers.Authorization;

/// <summary>A room whose devices are read or operated through the IoT emulator.</summary>
/// <param name="RoomId">The room.</param>
/// <param name="HotelId">The hotel the room belongs to (from Accommodations).</param>
public sealed record DeviceRoom(int RoomId, int HotelId);

/// <summary>Requirement "the requester may read/operate the devices of this room".</summary>
public sealed class RoomDeviceAccessRequirement : IAuthorizationRequirement
{
    public static readonly RoomDeviceAccessRequirement Instance = new();

    private RoomDeviceAccessRequirement() { }
}

/// <summary>
///     R5 (team decision, US-11): a guest reaches only the room of their Confirmed booking that is in effect today;
///     hotel administrators and maintenance reach the rooms of their hotel; a chain administrator reaches every
///     room. Which roles may call each endpoint is decided by the policies of <see cref="Policies"/>.
/// </summary>
public sealed class RoomDeviceAuthorizationHandler(IBookingsContextFacade bookingsContextFacade, TimeProvider timeProvider)
    : AuthorizationHandler<RoomDeviceAccessRequirement, DeviceRoom>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoomDeviceAccessRequirement requirement, DeviceRoom room)
    {
        var user = context.User;

        if (user.IsChainAdmin())
        {
            context.Succeed(requirement);
            return;
        }

        if (user.IsGuest())
        {
            var today = timeProvider.GetUtcNow().UtcDateTime.Date;
            if (await bookingsContextFacade.HasCurrentConfirmedStayAsync(user.GetUserId(), room.RoomId, today))
                context.Succeed(requirement);
            return;
        }

        // Hotel administrators and maintenance staff: rooms of the hotel they are assigned to.
        if (user.GetHotelId() == room.HotelId)
            context.Succeed(requirement);
    }
}
