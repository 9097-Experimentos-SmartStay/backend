using BackendAwSmartstay.API.Analytics.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Analytics.Domain.Repositories;

/// <summary>
/// Interface for retrieving analytical data.
/// </summary>
public interface IAnalyticsRepository
{
    /// <summary>
    /// Calculates performance metrics based on current data.
    /// </summary>
    /// <returns>A PerformanceMetrics aggregate.</returns>
    Task<PerformanceMetrics> GetMonthlyMetricsAsync();
}