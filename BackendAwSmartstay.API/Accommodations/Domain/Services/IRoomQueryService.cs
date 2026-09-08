using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

public interface IRoomQueryService
{

    Task<Room?> Handle(GetRoomByIdQuery query);

    Task<IEnumerable<Room>> Handle(GetAllRoomsQuery query);

    Task<IEnumerable<Room>> Handle(GetRoomsByTypeQuery query);
}

