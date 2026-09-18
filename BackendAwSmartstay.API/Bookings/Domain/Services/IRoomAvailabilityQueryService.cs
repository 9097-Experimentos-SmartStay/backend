using BackendAwSmartstay.API.Bookings.Domain.Model.Queries;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Services;

public interface IRoomAvailabilityQueryService
{
    Task<IReadOnlyList<AvailableRoom>> Handle(GetAvailableRoomsQuery query);
}
