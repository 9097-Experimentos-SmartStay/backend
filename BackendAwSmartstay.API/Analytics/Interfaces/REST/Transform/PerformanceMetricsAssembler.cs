using BackendAwSmartstay.API.Analytics.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Analytics.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Analytics.Interfaces.REST.Transform;

public static class PerformanceMetricsAssembler
{
    public static PerformanceMetricsResource ToResourceFromEntity(PerformanceMetrics entity)
    {
        return new PerformanceMetricsResource(
            entity.TotalRevenue,
            entity.TotalBookings,
            entity.OccupancyRate,
            entity.CancelledBookings,
            entity.DateGenerated.ToString("yyyy-MM-dd HH:mm:ss")
        );
    }
}