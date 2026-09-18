using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

namespace BackendAwSmartstay.API.Accommodations.Application.ACL;

public class AccommodationsContextFacade(
    IHotelQueryService hotelQueryService,
    IRoomQueryService roomQueryService) : IAccommodationsContextFacade
{
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
}
