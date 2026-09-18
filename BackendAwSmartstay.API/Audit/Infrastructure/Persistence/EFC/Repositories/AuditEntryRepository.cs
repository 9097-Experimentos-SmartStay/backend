using BackendAwSmartstay.API.Audit.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Audit.Domain.Model.Queries;
using BackendAwSmartstay.API.Audit.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Audit.Infrastructure.Persistence.EFC.Repositories;

public class AuditEntryRepository(AppDbContext context) : IAuditEntryRepository
{
    public async Task AddAsync(AuditEntry entry) => await context.Set<AuditEntry>().AddAsync(entry);

    public async Task<PagedResult<AuditEntry>> SearchAsync(GetAuditEntriesQuery query)
    {
        var entries = context.Set<AuditEntry>().AsNoTracking();

        if (!query.Scope.AllHotels)
        {
            var hotelId = query.Scope.HotelId;
            entries = hotelId is null ? entries.Where(_ => false) : entries.Where(e => e.HotelId == hotelId);
        }
        if (query.UserId is { } userId)
            entries = entries.Where(e => e.ActorUserId == userId || e.TargetUserId == userId);
        if (query.Action is { } action)
            entries = entries.Where(e => e.Action == action);
        if (query.From is { } from)
            entries = entries.Where(e => e.OccurredAt >= from);
        if (query.To is { } to)
            entries = entries.Where(e => e.OccurredAt <= to);

        var total = await entries.CountAsync();
        var items = await entries
            .OrderByDescending(e => e.OccurredAt).ThenByDescending(e => e.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<AuditEntry>(items, query.Page, query.PageSize, total);
    }
}
