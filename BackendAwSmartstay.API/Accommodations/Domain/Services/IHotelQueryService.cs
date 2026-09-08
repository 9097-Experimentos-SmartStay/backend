using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Accommodations.Domain.Services;

public interface IHotelQueryService
{
    Task<Hotel?> Handle(GetHotelByIdQuery query);
    Task<IEnumerable<Hotel>> Handle(GetAllHotelsQuery query);
}