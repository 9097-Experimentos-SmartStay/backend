using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.QueryServices;

/// <summary>
/// Booking queries. Visibility follows the Booking aggregate (<see cref="Booking.IsVisibleTo"/>): a guest sees their
/// own bookings, staff those of their hotel and a chain administrator every booking (R4).
/// </summary>
public class BookingQueryService(
    IBookingRepository bookingRepository,
    IGuestProfilesContextFacade guestProfilesContextFacade,
    IAccommodationsContextFacade accommodationsContextFacade)
    : IBookingQueryService
{
    public Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IEnumerable<Booking> bookings) =>
        accommodationsContextFacade.FetchRoomNumbersAsync(bookings.Select(booking => booking.RoomId).Distinct().ToList());

    /// <summary>Longest period the calendar shows at once.</summary>
    public const int MaxCalendarDays = 92;

    public async Task<Booking?> Handle(GetBookingByIdQuery query)
    {
        var booking = await bookingRepository.FindByIdAsync(query.BookingId);
        if (booking is null || query.Requester is null) return booking;

        return booking.IsVisibleTo(await ResolveGuestProfileAsync(query.Requester)) ? booking : null;
    }

    public async Task<IEnumerable<Booking>> Handle(GetBookingsQuery query)
    {
        var requester = query.Requester;
        if (!requester.IsGuest)
        {
            if (requester.AllHotels) return await bookingRepository.ListNewestFirstAsync(null);
            return requester.StaffHotelId is { } hotelId ? await bookingRepository.ListNewestFirstAsync(hotelId) : [];
        }

        requester = await ResolveGuestProfileAsync(requester);
        return await bookingRepository.FindByOwnerAsync(requester.UserId, requester.GuestProfileId);
    }

    public async Task<IEnumerable<Booking>> Handle(GetBookingsByRoomIdQuery query)
    {
        var bookings = await bookingRepository.FindByRoomIdAsync(query.RoomId);
        return bookings.Where(booking => booking.IsVisibleTo(query.Requester));
    }

    public async Task<BookingCalendar> Handle(GetBookingCalendarQuery query)
    {
        var requester = query.Requester;
        if (query.Window.Nights > MaxCalendarDays)
            throw new DomainValidationException($"The calendar shows at most {MaxCalendarDays} days at once.");

        var hotelId = query.HotelId ?? (requester.AllHotels ? null : requester.StaffHotelId);
        if (hotelId is null && !requester.AllHotels)
            throw new BookingOutsideHotelScopeException();
        if (hotelId is { } id && !requester.OperatesHotel(id))
            throw new BookingOutsideHotelScopeException();

        var bookings = await bookingRepository.ListActiveOverlappingAsync(hotelId, query.Window);
        return BookingCalendar.Build(hotelId, query.Window, bookings);
    }

    private async Task<BookingRequester> ResolveGuestProfileAsync(BookingRequester requester) =>
        requester.IsGuest
            ? requester.WithGuestProfile(await guestProfilesContextFacade.FetchGuestProfileIdByUserIdAsync(requester.UserId))
            : requester;
}
