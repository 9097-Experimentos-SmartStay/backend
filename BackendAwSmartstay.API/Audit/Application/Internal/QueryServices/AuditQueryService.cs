using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Audit.Domain.Model.Queries;
using BackendAwSmartstay.API.Audit.Domain.Repositories;
using BackendAwSmartstay.API.Audit.Domain.Services;

namespace BackendAwSmartstay.API.Audit.Application.Internal.QueryServices;

public class AuditQueryService(IAuditEntryRepository auditEntryRepository) : IAuditQueryService
{
    public Task<PagedResult<AuditEntry>> Handle(GetAuditEntriesQuery query) => auditEntryRepository.SearchAsync(query);
}
