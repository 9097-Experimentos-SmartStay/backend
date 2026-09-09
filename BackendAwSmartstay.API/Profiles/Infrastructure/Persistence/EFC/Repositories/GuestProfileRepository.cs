using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

public class GuestProfileRepository(AppDbContext context)
    : BaseRepository<GuestProfile>(context), IGuestProfileRepository
{
    public async Task<GuestProfile?> FindByIdAsync(GuestProfileId id)
    {
        return await Context.GuestProfiles
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GuestProfile?> FindByEmailAsync(EmailAddress email)
    {
        return await Context.GuestProfiles
            .FirstOrDefaultAsync(g => g.Email != null && g.Email.Address == email.Address);
    }

    public async Task<GuestProfile?> FindByUserIdAsync(UserId userId)
    {
        return await Context.GuestProfiles
            .FirstOrDefaultAsync(g => g.UserId == userId);
    }

    public async Task<GuestProfile?> FindByDocumentAsync(IdentificationDocument document)
    {
        return await Context.GuestProfiles
            .FirstOrDefaultAsync(g => g.Document != null 
                                      && g.Document.Type == document.Type 
                                      && g.Document.Number == document.Number);
    }

    public async Task<bool> ExistsByEmailAsync(EmailAddress email)
    {
        return await Context.GuestProfiles
            .AnyAsync(g => g.Email != null && g.Email.Address == email.Address);
    }

    public async Task<bool> ExistsByUserIdAsync(UserId userId)
    {
        return await Context.GuestProfiles
            .AnyAsync(g => g.UserId == userId);
    }
}
