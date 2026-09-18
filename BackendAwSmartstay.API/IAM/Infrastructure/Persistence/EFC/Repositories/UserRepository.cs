using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> FindByEmailAsync(Email email)
    {
        return await Context.Set<User>().FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(Email email)
    {
        return await Context.Set<User>().AnyAsync(user => user.Email == email);
    }

    public async Task<int> CountActiveByRoleAsync(string role)
    {
        var roleVo = new Role(role);
        return await Context.Set<User>()
            .CountAsync(user => user.Role == roleVo && user.Status == UserStatus.Active);
    }

    public async Task<IReadOnlyList<User>> ListActiveByRolesAsync(IReadOnlyCollection<Role> roles)
    {
        // Filtered by role in memory: a collection of converted value objects is not translated reliably to SQL.
        var active = await Context.Set<User>().Where(user => user.Status == UserStatus.Active).ToListAsync();
        return active.Where(user => roles.Contains(user.Role)).ToList();
    }
}
