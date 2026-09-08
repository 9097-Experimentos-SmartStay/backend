using BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Profiles.Domain.Model.Queries;
using BackendAwSmartstay.API.Profiles.Domain.Repositories;
using BackendAwSmartstay.API.Profiles.Domain.Services;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;

/// <summary>
///     Profile query service
/// </summary>
/// <param name="profileRepository">
///     Profile repository
/// </param>
public class ProfileQueryService(IProfileRepository profileRepository) : IProfileQueryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query)
    {
        return await profileRepository.ListAsync();
    }

    /// <inheritdoc />
    public async Task<Profile?> Handle(GetProfileByEmailQuery query)
    {
        return await profileRepository.FindProfileByEmailAsync(query.Email);
    }

    /// <inheritdoc />
    public async Task<Profile?> Handle(GetProfileByIdQuery query)
    {
        return await profileRepository.FindByIdAsync(query.ProfileId);
    }
}