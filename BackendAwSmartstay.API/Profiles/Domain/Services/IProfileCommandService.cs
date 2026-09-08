using BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Profiles.Domain.Model.Commands;

namespace BackendAwSmartstay.API.Profiles.Domain.Services;

/// <summary>
///     Profile command service interface
/// </summary>
public interface IProfileCommandService
{
    /// <summary>
    ///     Handle create profile command
    /// </summary>
    /// <param name="command">
    ///     The <see cref="CreateProfileCommand" /> command
    /// </param>
    /// <returns>
    ///     The <see cref="Profile" /> object with the created profile
    /// </returns>
    Task<Profile?> Handle(CreateProfileCommand command);
}