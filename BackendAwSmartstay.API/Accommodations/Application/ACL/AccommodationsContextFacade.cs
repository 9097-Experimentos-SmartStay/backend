using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

namespace BackendAwSmartstay.API.Accommodations.Application.ACL;

public class AccommodationsContextFacade(
    IHotelQueryService hotelQueryService,
    IRoomQueryService roomQueryService,
    IRoomRepository roomRepository) : IAccommodationsContextFacade
{
    public async Task<bool> LockRoomForBookingAsync(int roomId) =>
        roomId > 0 && await roomRepository.FindByIdForUpdateAsync(roomId) is not null;

    public async Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? hotelId)
    {
        var rooms = await roomQueryService.Handle(new GetRoomsOfferedForBookingQuery(hotelId));
        return rooms.Select(room => new RoomOffer(room.Id, room.HotelId, room.RoomTypeId, room.RoomType?.Name ?? string.Empty,
            room.Price, room.Description, room.Amenities, room.Status.ToString())).ToList();
    }

    public async Task<bool> HotelExistsAsync(int hotelId)
    {
        if (hotelId <= 0)
            return false;

        var query = new GetHotelByIdQuery(hotelId);
        var hotel = await hotelQueryService.Handle(query);
        return hotel != null;
    }

    public async Task<bool> RoomExistsAsync(int roomId)
    {
        return await FetchRoomPricePerNightAsync(roomId) is not null;
    }

    public async Task<decimal?> FetchRoomPricePerNightAsync(int roomId)
    {
        if (roomId <= 0)
            return null;

        var room = await roomQueryService.Handle(new GetRoomByIdQuery(roomId));
        return room?.Price;
    }

    public async Task<int?> FetchHotelIdOfRoomAsync(int roomId)
    {
        if (roomId <= 0)
            return null;

        var room = await roomQueryService.Handle(new GetRoomByIdQuery(roomId));
        return room?.HotelId;
    }
}
