using BackendAwSmartstay.API.Analytics.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Analytics.Domain.Model.Queries;
using BackendAwSmartstay.API.Analytics.Domain.Repositories;
using BackendAwSmartstay.API.Analytics.Domain.Services;

namespace BackendAwSmartstay.API.Analytics.Application.Internal.QueryServices;

/// <summary>
/// Service to handle analytics data retrieval.
/// </summary>
public class AnalyticsQueryService(IAnalyticsRepository analyticsRepository) : IAnalyticsQueryService
{
    public async Task<PerformanceMetrics> Handle(GetMonthlyPerformanceQuery query)
    {
        // Here we could add caching layers or more complex logic
        return await analyticsRepository.GetMonthlyMetricsAsync();
    }
}