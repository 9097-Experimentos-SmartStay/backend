using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Accommodations.Domain.Repositories;

/// <summary>
/// Repository interface for managing Room aggregates.
/// </summary>
public interface IRoomRepository : IBaseRepository<Room>
{
}
