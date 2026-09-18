using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class MfaRecoveryCodeRepository(AppDbContext context) : BaseRepository<MfaRecoveryCode>(context), IMfaRecoveryCodeRepository
{
    public async Task<IReadOnlyList<MfaRecoveryCode>> ListUnusedByUserAsync(int userId) =>
        await Context.Set<MfaRecoveryCode>().Where(c => c.UserId == userId && c.UsedAt == null).ToListAsync();

    public async Task RemoveAllOfUserAsync(int userId)
    {
        var codes = await Context.Set<MfaRecoveryCode>().Where(c => c.UserId == userId).ToListAsync();
        Context.Set<MfaRecoveryCode>().RemoveRange(codes);
    }
}
