using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;

public class GuestProfileQueryService(IGuestProfileRepository guestProfileRepository) : IGuestProfileQueryService
{
    public async Task<GuestProfile?> Handle(GetGuestProfileByIdQuery query)
    {
        return await guestProfileRepository.FindByIdAsync(query.ProfileId);
    }

    public async Task<GuestProfile?> Handle(GetGuestProfileByEmailQuery query)
    {
        return await guestProfileRepository.FindByEmailAsync(query.Email);
    }

    public async Task<GuestProfile?> Handle(GetGuestProfileByUserIdQuery query)
    {
        return await guestProfileRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<IEnumerable<GuestProfile>> Handle(GetAllGuestProfilesQuery query)
    {
        return await guestProfileRepository.ListAsync();
    }
}
