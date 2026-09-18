using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Interfaces.ACL;

namespace BackendAwSmartstay.API.Bookings.Application.ACL;

public class RoomReservationsFacade(IBookingRepository bookingRepository) : IRoomReservationsFacade
{
    public Task<IReadOnlyDictionary<int, int>> CountActiveBookingsAsync(IReadOnlyCollection<int> roomIds) =>
        bookingRepository.CountActiveByRoomAsync(roomIds);
}
