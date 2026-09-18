using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.QueryServices;

/// <summary>
///     Combines the rooms offered by the Accommodations context (through its ACL facade) with the active bookings of
///     this context to answer which rooms are free for a stay.
/// </summary>
public class RoomAvailabilityQueryService(
    IAccommodationsContextFacade accommodationsContextFacade,
    IBookingRepository bookingRepository,
    HotelCalendar calendar) : IRoomAvailabilityQueryService
{
    public async Task<IReadOnlyList<AvailableRoom>> Handle(GetAvailableRoomsQuery query)
    {
        // US-51 scenario 4: a stay cannot start in the past.
        query.Dates.EnsureNotInThePast(calendar.Today);

        var offered = await accommodationsContextFacade.FetchRoomsOfferedForBookingAsync(query.HotelId);
        if (offered.Count == 0) return [];

        var booked = await bookingRepository.FindRoomIdsWithActiveBookingOverlappingAsync(
            offered.Select(room => room.RoomId).ToList(), query.Dates);

        return offered
            .Where(room => !booked.Contains(room.RoomId))
            .Select(room => new AvailableRoom(room, query.Dates))
            .ToList();
    }
}
