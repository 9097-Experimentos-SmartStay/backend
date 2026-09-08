using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

public interface IUserQueryService
{
    Task<User?> Handle(GetUserByIdQuery query);
    Task<IEnumerable<User>> Handle(GetUsersByScopeQuery query);
    Task<User?> Handle(GetUserByUsernameQuery query);
}