using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

/// <summary>
///     Domain service for R1 (US-07 "avoid overbooking", canvas "Bookings", glossary "Overbooking"): a room cannot
///     have two active bookings (Pending or Confirmed) whose stays share a night.
/// </summary>
public class RoomAvailabilityService(IBookingRepository bookingRepository)
{
    /// <exception cref="RoomNotAvailableException">An active booking of the room overlaps the requested dates.</exception>
    public async Task EnsureRoomIsAvailableAsync(int roomId, DateRange dates)
    {
        if (await bookingRepository.ExistsActiveBookingOverlappingAsync(roomId, dates))
            throw new RoomNotAvailableException(roomId, dates);
    }
}
