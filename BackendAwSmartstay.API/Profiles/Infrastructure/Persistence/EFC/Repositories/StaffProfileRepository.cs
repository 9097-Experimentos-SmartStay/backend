using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

public class StaffProfileRepository(AppDbContext context)
    : BaseRepository<StaffProfile>(context), IStaffProfileRepository
{
    public async Task<StaffProfile?> FindByIdAsync(StaffProfileId id)
    {
        return await Context.StaffProfiles
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StaffProfile?> FindByUserIdAsync(UserId userId)
    {
        return await Context.StaffProfiles
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<StaffProfile?> FindByEmployeeCodeAsync(EmployeeCode code)
    {
        return await Context.StaffProfiles
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<IEnumerable<StaffProfile>> FindByTargetIdAsync(TargetId targetId)
    {
        return await Context.StaffProfiles
            .Include(s => s.Assignments)
            .Where(sp => sp.Assignments.Any(a => a.TargetId == targetId))
            .ToListAsync();
    }

    public async Task<bool> ExistsByEmployeeCodeAsync(EmployeeCode code)
    {
        return await Context.StaffProfiles
            .AnyAsync(s => s.Code == code);
    }

    public async Task<bool> ExistsByUserIdAsync(UserId userId)
    {
        return await Context.StaffProfiles
            .AnyAsync(s => s.UserId == userId);
    }

    public new async Task<IEnumerable<StaffProfile>> ListAsync()
    {
        return await Context.StaffProfiles
            .Include(s => s.Assignments)
            .ToListAsync();
    }
}
