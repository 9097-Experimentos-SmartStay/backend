using BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Profiles.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     Profile repository implementation
/// </summary>
/// <param name="context">
///     The database context
/// </param>
public class ProfileRepository(AppDbContext context)
    : BaseRepository<Profile>(context), IProfileRepository
{
    /// <inheritdoc />
    public async Task<Profile?> FindProfileByEmailAsync(EmailAddress email)
    {
        return await Context.Set<Profile>().FirstOrDefaultAsync(p => p.Email == email);
    }
}