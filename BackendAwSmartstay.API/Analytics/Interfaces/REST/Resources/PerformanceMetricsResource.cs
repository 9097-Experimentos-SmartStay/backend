namespace BackendAwSmartstay.API.Analytics.Interfaces.REST.Resources;

/// <summary>
///     Resource containing the performance metrics for the dashboard.
/// </summary>
public record PerformanceMetricsResource(
    decimal TotalRevenue,
    int TotalBookings,
    double OccupancyRate,
    int CancelledBookings,
    string GeneratedAt
);