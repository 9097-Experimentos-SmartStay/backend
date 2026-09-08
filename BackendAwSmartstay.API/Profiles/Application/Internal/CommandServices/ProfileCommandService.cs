using BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Profiles.Domain.Model.Commands;
using BackendAwSmartstay.API.Profiles.Domain.Repositories;
using BackendAwSmartstay.API.Profiles.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Profile command service
/// </summary>
/// <param name="profileRepository">
///     Profile repository
/// </param>
/// <param name="unitOfWork">
///     Unit of work
/// </param>
public class ProfileCommandService(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
    : IProfileCommandService
{
    /// <inheritdoc />
    public async Task<Profile?> Handle(CreateProfileCommand command)
    {
        var profile = new Profile(command);
        try
        {
            await profileRepository.AddAsync(profile);
            await unitOfWork.CompleteAsync();
            return profile;
        }
        catch (Exception e)
        {
            // Log error
            return null;
        }
    }
}