using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;

public static class CreateBookingCommandFromResourceAssembler
{
    /// <summary>
    ///     Builds the command. The Booking aggregate decides which fields apply: a guest always books for
    ///     themselves, so <c>userId</c> and <c>guestProfileId</c> only matter for desk bookings.
    /// </summary>
    public static CreateBookingCommand ToCommandFromResource(CreateBookingResource resource, BookingRequester requester)
    {
        return new CreateBookingCommand(
            requester,
            resource.RoomId,
            resource.GuestName,
            resource.GuestEmail,
            resource.CheckInDate,
            resource.CheckOutDate,
            resource.UserId,
            resource.GuestProfileId);
    }
}
