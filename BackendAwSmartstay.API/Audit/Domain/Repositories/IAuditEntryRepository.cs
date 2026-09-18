using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Audit.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Audit.Domain.Repositories;

/// <summary>Append-only store of the audit log.</summary>
public interface IAuditEntryRepository
{
    Task AddAsync(AuditEntry entry);

    Task<PagedResult<AuditEntry>> SearchAsync(GetAuditEntriesQuery query);
}
