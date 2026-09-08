using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;

namespace BackendAwSmartstay.API.IAM.Application.Internal.QueryServices;

public class UserQueryService(
    IUserRepository userRepository,
    IUserScopeService userScopeService) : IUserQueryService
{
    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        var target = await userRepository.FindByIdAsync(query.Id);
        if (target == null) return null;

        // Scope verification
        if (query.ActorUserId.HasValue)
        {
            var actor = await userRepository.FindByIdAsync(query.ActorUserId.Value);
            if (actor == null || !userScopeService.CanAccessUser(actor, target))
                return null; 
        }

        return target;
    }

    public async Task<IEnumerable<User>> Handle(GetUsersByScopeQuery query)
    {
        var actor = await userRepository.FindByIdAsync(query.ActorUserId);
        if (actor == null) return Enumerable.Empty<User>();

        var allUsers = await userRepository.ListAsync();
        
        // Reutilizamos la lógica centralizada del dominio. DDD 10/10.
        return allUsers.Where(target => userScopeService.CanAccessUser(actor, target));
    }

    public async Task<User?> Handle(GetUserByUsernameQuery query)
    {
        var username = new Username(query.Username);
        return await userRepository.FindByUsernameAsync(username);
    }
}