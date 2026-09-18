using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Application.ACL;

public class BookingsContextFacade(
    IBookingQueryService bookingQueryService,
    IBookingCommandService bookingCommandService) : IBookingsContextFacade
{
    public async Task<BookingSnapshot?> FetchBookingAsync(int bookingId, int? guestUserId = null)
    {
        var booking = guestUserId.HasValue
            ? await bookingQueryService.Handle(new GetOwnedBookingByIdQuery(bookingId, guestUserId.Value))
            : await bookingQueryService.Handle(new GetBookingByIdQuery(bookingId));

        return booking is null ? null : ToSnapshot(booking);
    }

    public async Task<bool> ConfirmBookingAsync(int bookingId)
    {
        var booking = await bookingCommandService.Handle(new ConfirmBookingCommand(bookingId));
        return booking is not null;
    }

    private static BookingSnapshot ToSnapshot(Booking booking) => new(
        booking.Id,
        booking.RoomId,
        booking.CheckInDate,
        booking.CheckOutDate,
        booking.Nights,
        booking.Status.ToString(),
        booking.CanBePaid);
}
