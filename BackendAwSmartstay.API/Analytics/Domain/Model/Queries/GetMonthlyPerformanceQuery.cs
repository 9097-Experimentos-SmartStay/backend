namespace BackendAwSmartstay.API.Analytics.Domain.Model.Queries;

/// <summary>
/// Query to request performance metrics for the current month.
/// </summary>
/// <param name="HotelId">The hotel the metrics are about; null only with <paramref name="WholeChain"/>.</param>
/// <param name="WholeChain">
///     True for a chain admin without a hotel filter: every hotel of the chain. A hotel admin always sees only
///     their own hotel (D2), and an admin without a hotel sees empty metrics.
/// </param>
public record GetMonthlyPerformanceQuery(int? HotelId, bool WholeChain);
