using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;

public static class CreateBookingCommandFromResourceAssembler
{
    public static CreateBookingCommand ToCommandFromResource(CreateBookingResource resource)
    {
        return new CreateBookingCommand(
            resource.RoomId,
            resource.GuestName,
            resource.GuestEmail,
            resource.CheckInDate,
            resource.CheckOutDate);
    }
}

