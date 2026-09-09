using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;

public class StaffProfileQueryService(IStaffProfileRepository staffProfileRepository) : IStaffProfileQueryService
{
    public async Task<StaffProfile?> Handle(GetStaffProfileByIdQuery query)
    {
        return await staffProfileRepository.FindByIdAsync(query.ProfileId);
    }

    public async Task<StaffProfile?> Handle(GetStaffProfileByUserIdQuery query)
    {
        return await staffProfileRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<StaffProfile?> Handle(GetStaffProfileByEmployeeCodeQuery query)
    {
        return await staffProfileRepository.FindByEmployeeCodeAsync(query.Code);
    }

    public async Task<IEnumerable<StaffProfile>> Handle(GetStaffProfilesByHotelIdQuery query)
    {
        return await staffProfileRepository.FindByTargetIdAsync(query.HotelId);
    }

    public async Task<IEnumerable<StaffProfile>> Handle(GetAllStaffProfilesQuery query)
    {
        return await staffProfileRepository.ListAsync();
    }
}
