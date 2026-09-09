using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;

public interface IGuestProfileQueryService
{
    Task<GuestProfile?> Handle(GetGuestProfileByIdQuery query);
    Task<GuestProfile?> Handle(GetGuestProfileByEmailQuery query);
    Task<GuestProfile?> Handle(GetGuestProfileByUserIdQuery query);
    Task<IEnumerable<GuestProfile>> Handle(GetAllGuestProfilesQuery query);
}
