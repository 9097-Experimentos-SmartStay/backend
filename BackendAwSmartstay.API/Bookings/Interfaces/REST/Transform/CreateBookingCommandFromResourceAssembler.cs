using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;

public static class CreateBookingCommandFromResourceAssembler
{
    public static CreateBookingCommand ToCommandFromResource(CreateBookingResource resource)
    {
        return ToCommandFromResource(resource, resource.UserId, resource.GuestProfileId,
            resource.GuestName, resource.GuestEmail);
    }

    /// <summary>
    ///     Builds the command with identity fields resolved by the controller (e.g. taken from the token for guests).
    /// </summary>
    public static CreateBookingCommand ToCommandFromResource(CreateBookingResource resource, int? userId,
        Guid? guestProfileId, string guestName, string guestEmail)
    {
        return new CreateBookingCommand(
            resource.RoomId,
            guestName,
            guestEmail,
            resource.CheckInDate,
            resource.CheckOutDate,
            userId,
            guestProfileId);
    }
}

