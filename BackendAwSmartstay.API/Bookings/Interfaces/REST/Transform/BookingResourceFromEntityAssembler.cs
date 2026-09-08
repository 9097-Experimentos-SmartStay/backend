using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;

public static class BookingResourceFromEntityAssembler
{

    public static BookingResource ToResourceFromEntity(Booking entity)
    {
        return new BookingResource(
            entity.Id,
            entity.RoomId,
            entity.GuestName,
            entity.GuestEmail,
            entity.CheckInDate,
            entity.CheckOutDate,
            entity.Status.ToString());
    }
}

