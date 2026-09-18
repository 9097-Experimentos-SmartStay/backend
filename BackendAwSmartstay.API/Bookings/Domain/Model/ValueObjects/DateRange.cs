using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>
///     The stay dates of a booking (<c>dates: DateRange</c> in the report's Booking component). Stays are counted in
///     calendar days: the time of day is ignored and the check-out must be at least one day after the check-in
///     (canvas "Bookings": a booking must have valid check-in and check-out dates).
/// </summary>
public sealed record DateRange
{
    public DateRange(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut.Date <= checkIn.Date)
            throw new DomainValidationException("The check-out date must be at least one day after the check-in date.");

        CheckIn = DateTime.SpecifyKind(checkIn.Date, DateTimeKind.Unspecified);
        CheckOut = DateTime.SpecifyKind(checkOut.Date, DateTimeKind.Unspecified);
    }

    public DateTime CheckIn { get; }
    public DateTime CheckOut { get; }

    /// <summary>Number of nights (calendar days between check-in and check-out).</summary>
    public int Nights => (CheckOut - CheckIn).Days;

    /// <summary>
    ///     True when both stays share at least one night. Back-to-back stays (one checks out the day the other
    ///     checks in) do not overlap.
    /// </summary>
    public bool Overlaps(DateRange other) => CheckIn < other.CheckOut && other.CheckIn < CheckOut;

    /// <summary>True when <paramref name="day"/> is one of the nights of the stay (check-in day included, check-out day excluded).</summary>
    public bool Includes(DateTime day) => CheckIn <= day.Date && day.Date < CheckOut;

    public override string ToString() => $"{CheckIn:yyyy-MM-dd}..{CheckOut:yyyy-MM-dd}";
}
