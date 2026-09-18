using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

public interface IUserQueryService
{
    Task<User?> Handle(GetUserByIdQuery query);
    Task<IEnumerable<User>> Handle(GetUsersByScopeQuery query);
    Task<User?> Handle(GetUserByEmailQuery query);

    /// <summary>The signed-in user, or null when the account no longer exists.</summary>
    Task<User?> Handle(GetCurrentUserQuery query);

    /// <summary>Whether a token issued to the user is still a valid session (used by the authentication handler).</summary>
    Task<UserSession> Handle(GetUserSessionQuery query);
}