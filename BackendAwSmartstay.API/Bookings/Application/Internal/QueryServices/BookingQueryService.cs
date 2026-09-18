using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.QueryServices;

/// <summary>
/// Booking queries. Visibility follows the Booking aggregate (<see cref="Booking.IsVisibleTo"/>).
/// </summary>
public class BookingQueryService(
    IBookingRepository bookingRepository,
    IGuestProfilesContextFacade guestProfilesContextFacade)
    : IBookingQueryService
{
    public async Task<Booking?> Handle(GetBookingByIdQuery query)
    {
        var booking = await bookingRepository.FindByIdAsync(query.BookingId);
        if (booking is null || query.Requester is null) return booking;

        return booking.IsVisibleTo(await ResolveGuestProfileAsync(query.Requester)) ? booking : null;
    }

    public async Task<IEnumerable<Booking>> Handle(GetBookingsQuery query)
    {
        if (!query.Requester.IsGuest)
            return await bookingRepository.ListNewestFirstAsync();

        var requester = await ResolveGuestProfileAsync(query.Requester);
        return await bookingRepository.FindByOwnerAsync(requester.UserId, requester.GuestProfileId);
    }

    public async Task<IEnumerable<Booking>> Handle(GetBookingsByRoomIdQuery query)
    {
        return await bookingRepository.FindByRoomIdAsync(query.RoomId);
    }

    private async Task<BookingRequester> ResolveGuestProfileAsync(BookingRequester requester) =>
        requester.IsGuest
            ? requester.WithGuestProfile(await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(requester.UserId))
            : requester;
}
