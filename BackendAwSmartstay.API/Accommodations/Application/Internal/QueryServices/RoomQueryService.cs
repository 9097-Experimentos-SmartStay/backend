using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;

namespace BackendAwSmartstay.API.Accommodations.Application.Internal.QueryServices;


public class RoomQueryService(IRoomRepository roomRepository)
    : IRoomQueryService
{
    /// <summary>
    /// Provides query handling services for retrieving room information, 
    /// including fetching rooms by ID, type, or listing all available rooms.
    /// </summary>
    public async Task<Room?> Handle(GetRoomByIdQuery query)
    {
        // Retrieves a room based on the provided room identifier.
        return await roomRepository.FindByIdAsync(query.RoomId);  
       
    }
  
    /// <summary>
    /// Retrieves a list of all rooms available in the system.
    /// </summary>
    /// <param name="query">Query used to trigger the operation.</param>
    /// <returns>An enumerable collection of <see cref="Room"/>.</returns>
    public async Task<IEnumerable<Room>> Handle(GetAllRoomsQuery query)
    {
        return await roomRepository.ListAsync();
    }

    public Task<IEnumerable<Room>> Handle(GetRoomsOfferedForBookingQuery query) =>
        roomRepository.FindOfferedForBookingAsync(query.HotelId);

    public async Task<IEnumerable<Room>> Handle(GetRoomsByTypeQuery query)
    {
        // Retrieves all rooms that belong to a specific room type.
        var rooms = await roomRepository.ListAsync();
        return rooms.Where(r => r.RoomTypeId == query.RoomTypeId);
    }        

 
}

