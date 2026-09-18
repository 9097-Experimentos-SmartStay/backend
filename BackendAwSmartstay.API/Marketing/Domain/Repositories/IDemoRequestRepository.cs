using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.Queries;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.Marketing.Domain.Repositories;

public interface IDemoRequestRepository : IBaseRepository<DemoRequest>
{
    /// <summary>Requests still Received that arrived at or before <paramref name="receivedAtOrBefore"/>.</summary>
    Task<IReadOnlyList<DemoRequest>> FindWaitingSinceAsync(DateTimeOffset receivedAtOrBefore);

    Task<DemoRequestPage> SearchAsync(GetDemoRequestsQuery query);
}
