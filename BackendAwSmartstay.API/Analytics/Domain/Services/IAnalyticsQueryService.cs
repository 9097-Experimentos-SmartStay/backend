using BackendAwSmartstay.API.Analytics.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Analytics.Domain.Model.Queries;

namespace BackendAwSmartstay.API.Analytics.Domain.Services;

/// <summary>
/// Service contract for handling analytics queries.
/// </summary>
public interface IAnalyticsQueryService
{
    Task<PerformanceMetrics> Handle(GetMonthlyPerformanceQuery query);
}