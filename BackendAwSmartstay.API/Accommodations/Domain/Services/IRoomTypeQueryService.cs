using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

public interface IRoomTypeQueryService
{

    Task<RoomType?> Handle(GetRoomTypeByIdQuery query);
    
    Task<IEnumerable<RoomType>> Handle(GetAllRoomTypesQuery query);
}

