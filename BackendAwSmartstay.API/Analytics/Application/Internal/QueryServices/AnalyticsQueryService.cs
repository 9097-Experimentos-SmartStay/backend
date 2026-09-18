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
        // Metrics are hotel-scoped: without a hotel (and not the whole chain) there is nothing to report.
        if (!query.WholeChain && query.HotelId is null) return new PerformanceMetrics();
        return await analyticsRepository.GetMonthlyMetricsAsync(query.HotelId);
    }
}