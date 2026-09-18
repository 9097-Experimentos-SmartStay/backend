using BackendAwSmartstay.API.Bookings.Domain.Model.Exceptions;
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
            throw new DomainValidationException(BookingErrorCodes.CheckOutNotAfterCheckIn, "The check-out date must be at least one day after the check-in date.");

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

    /// <summary>US-51 scenario 4: a stay cannot start before <paramref name="hotelToday"/> (the hotel's calendar date).</summary>
    public void EnsureNotInThePast(DateTime hotelToday)
    {
        if (CheckIn < hotelToday.Date)
            throw new DomainValidationException(BookingErrorCodes.CheckInInPast, "The check-in date cannot be in the past.");
    }

    public override string ToString() => $"{CheckIn:yyyy-MM-dd}..{CheckOut:yyyy-MM-dd}";
}
