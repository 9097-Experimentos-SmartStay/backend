using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Domain.Repositories;

/// <summary>Profile pictures of the users. <c>FindByIdAsync</c> takes the user id (it is the key).</summary>
public interface IUserAvatarRepository : IBaseRepository<UserAvatar>;
