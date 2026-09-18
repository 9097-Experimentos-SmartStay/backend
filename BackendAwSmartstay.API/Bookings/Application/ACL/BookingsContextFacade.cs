using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Commands;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Application.ACL;

public class BookingsContextFacade(
    IBookingQueryService bookingQueryService,
    IBookingCommandService bookingCommandService) : IBookingsContextFacade
{
    public async Task<BookingSnapshot?> FetchBookingAsync(int bookingId, int? guestUserId = null)
    {
        var requester = guestUserId.HasValue ? BookingRequester.Guest(guestUserId.Value, string.Empty) : null;
        var booking = await bookingQueryService.Handle(new GetBookingByIdQuery(bookingId, requester));
        return booking is null ? null : ToSnapshot(booking);
    }

    public async Task<bool> ConfirmBookingAsync(int bookingId)
    {
        await bookingCommandService.Handle(new ConfirmBookingCommand(bookingId));
        return true;
    }

    public async Task<bool> HasCurrentConfirmedStayAsync(int guestUserId, int roomId, DateTime day)
    {
        var requester = BookingRequester.Guest(guestUserId, string.Empty);
        // Only the guest's own bookings (account or guest profile) are returned by the query.
        var bookings = await bookingQueryService.Handle(new GetBookingsQuery(requester));
        return bookings.Any(booking => booking.IsConfirmedStayIn(roomId, day));
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
