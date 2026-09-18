using BackendAwSmartstay.API.Audit.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Audit.Domain.Model.Queries;

/// <summary>
///     A page of the audit log visible in <paramref name="Scope"/>, newest first, optionally filtered by user
///     (actor or target), action and date range.
/// </summary>
public sealed record GetAuditEntriesQuery
{
    public const int MaxPageSize = 100;

    public GetAuditEntriesQuery(AuditReadScope scope, int? userId, AuditAction? action,
        DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize)
    {
        if (page < 1) throw new DomainValidationException("page must be 1 or greater.");
        if (pageSize is < 1 or > MaxPageSize) throw new DomainValidationException($"pageSize must be between 1 and {MaxPageSize}.");
        if (from is not null && to is not null && from > to) throw new DomainValidationException("from must not be later than to.");

        Scope = scope;
        UserId = userId;
        Action = action;
        From = from;
        To = to;
        Page = page;
        PageSize = pageSize;
    }

    public AuditReadScope Scope { get; }
    public int? UserId { get; }
    public AuditAction? Action { get; }
    public DateTimeOffset? From { get; }
    public DateTimeOffset? To { get; }
    public int Page { get; }
    public int PageSize { get; }
}

/// <summary>A page of results and the total number of matching items.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
