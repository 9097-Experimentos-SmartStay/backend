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
    public async Task<User?> FindByUsernameAsync(string username)
    {
        var usernameVo = new Username(username);
        return await Context.Set<User>().FirstOrDefaultAsync(user => user.Username == usernameVo);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        var usernameVo = new Username(username);
        return await Context.Set<User>().AnyAsync(user => user.Username == usernameVo);
    }

    public async Task<int> CountActiveByRoleAsync(string role)
    {
        var roleVo = new Role(role);
        return await Context.Set<User>()
            .CountAsync(user => user.Role == roleVo && user.Status == UserStatus.Active);
    }
}