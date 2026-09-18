using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class RefreshTokenRepository(AppDbContext context) : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(string tokenHash) =>
        Context.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task<IReadOnlyList<RefreshToken>> ListUnrevokedByFamilyAsync(Guid familyId) =>
        await Context.Set<RefreshToken>().Where(t => t.FamilyId == familyId && t.RevokedAt == null).ToListAsync();

    public async Task<IReadOnlyList<RefreshToken>> ListUnrevokedByUserAsync(int userId) =>
        await Context.Set<RefreshToken>().Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync();
}
