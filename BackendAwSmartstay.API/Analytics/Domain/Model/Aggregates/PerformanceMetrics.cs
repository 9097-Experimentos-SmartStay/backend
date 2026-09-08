namespace BackendAwSmartstay.API.Analytics.Domain.Model.Aggregates;

/// <summary>
/// Represents a snapshot of the hotel's performance metrics.
/// </summary>
public class PerformanceMetrics
{
    public PerformanceMetrics()
    {
        DateGenerated = DateTime.UtcNow;
    }

    /// <summary>
    /// Total revenue generated in the period.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Number of bookings made.
    /// </summary>
    public int TotalBookings { get; set; }

    /// <summary>
    /// Percentage of rooms occupied (0-100).
    /// </summary>
    public double OccupancyRate { get; set; }

    /// <summary>
    /// Number of cancellations.
    /// </summary>
    public int CancelledBookings { get; set; }

    /// <summary>
    /// The date/time when this report was calculated.
    /// </summary>
    public DateTime DateGenerated { get; private set; }
}