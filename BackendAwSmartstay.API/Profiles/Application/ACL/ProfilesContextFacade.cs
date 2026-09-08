using BackendAwSmartstay.API.Profiles.Domain.Model.Commands;
using BackendAwSmartstay.API.Profiles.Domain.Model.Queries;
using BackendAwSmartstay.API.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Profiles.Domain.Services;
using BackendAwSmartstay.API.Profiles.Interfaces.ACL;

namespace BackendAwSmartstay.API.Profiles.Application.ACL;

/// <summary>
///     Facade for the profiles context
/// </summary>
/// <param name="profileCommandService">
///     The profile command service
/// </param>
/// <param name="profileQueryService">
///     The profile query service
/// </param>
public class ProfilesContextFacade(
    IProfileCommandService profileCommandService,
    IProfileQueryService profileQueryService
) : IProfilesContextFacade
{
    // inheritedDoc
    public async Task<int> CreateProfile(string firstName, string lastName, string email, string street, string number,
        string city,
        string postalCode, string country)
    {
        var createProfileCommand =
            new CreateProfileCommand(firstName, lastName, email, street, number, city, postalCode, country);
        var profile = await profileCommandService.Handle(createProfileCommand);
        return profile?.Id ?? 0;
    }

    // inheritedDoc
    public async Task<int> FetchProfileIdByEmail(string email)
    {
        var getProfileByEmailQuery = new GetProfileByEmailQuery(new EmailAddress(email));
        var profile = await profileQueryService.Handle(getProfileByEmailQuery);
        return profile?.Id ?? 0;
    }
}