using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
///     Domain service for R1 (US-07 "avoid overbooking", canvas "Bookings", glossary "Overbooking"): a room cannot
///     have two active bookings (Pending, Confirmed or CheckedIn) whose stays share a night.
/// </summary>
public class RoomAvailabilityService(IBookingRepository bookingRepository)
{
    /// <exception cref="RoomNotAvailableException">An active booking of the room overlaps the requested dates.</exception>
    /// <param name="roomId">The room.</param>
    /// <param name="dates">The stay.</param>
    /// <param name="excludingBookingId">A booking being moved does not conflict with itself.</param>
    public async Task EnsureRoomIsAvailableAsync(int roomId, DateRange dates, int? excludingBookingId = null)
    {
        if (await bookingRepository.ExistsActiveBookingOverlappingAsync(roomId, dates, excludingBookingId))
            throw new RoomNotAvailableException(roomId, dates);
    }
}
