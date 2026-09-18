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
    /// <param name="hotelId">Only this hotel's payments, bookings and rooms; null for every hotel.</param>
    /// <returns>A PerformanceMetrics aggregate.</returns>
    Task<PerformanceMetrics> GetMonthlyMetricsAsync(int? hotelId);
}