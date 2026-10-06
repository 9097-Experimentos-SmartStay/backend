using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class UserAvatarRepository(AppDbContext context) : BaseRepository<UserAvatar>(context), IUserAvatarRepository;
