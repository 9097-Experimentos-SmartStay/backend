using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Audit.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Audit.Domain.Services;

public interface IAuditQueryService
{
    Task<PagedResult<AuditEntry>> Handle(GetAuditEntriesQuery query);
}
