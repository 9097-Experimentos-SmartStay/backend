using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Marketing.Domain.Model.Queries;

/// <summary>Demo requests, newest first, optionally by status (sales team).</summary>
public sealed record GetDemoRequestsQuery
{
    public const int MaxPageSize = 100;

    public GetDemoRequestsQuery(DemoRequestStatus? status, int page, int pageSize)
    {
        if (page < 1) throw new DomainValidationException("page must be 1 or greater.");
        if (pageSize is < 1 or > MaxPageSize) throw new DomainValidationException($"pageSize must be between 1 and {MaxPageSize}.");
        Status = status;
        Page = page;
        PageSize = pageSize;
    }

    public DemoRequestStatus? Status { get; }
    public int Page { get; }
    public int PageSize { get; }
}

public record GetDemoRequestByIdQuery(int Id);

/// <summary>A page of demo requests and the total that match.</summary>
public sealed record DemoRequestPage(IReadOnlyList<Aggregates.DemoRequest> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
