using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;

/// <summary>The hotels' calendar: today's date and instants in the hotel time zone (<see cref="BookingPolicySettings.TimeZone"/>).</summary>
public class HotelCalendar(IOptions<BookingPolicySettings> settings, TimeProvider timeProvider)
{
    private TimeZoneInfo Zone => TimeZoneInfo.FindSystemTimeZoneById(settings.Value.TimeZone);

    public DateTimeOffset Now => timeProvider.GetUtcNow();

    /// <summary>The hotel's calendar date right now.</summary>
    public DateTime Today => TimeZoneInfo.ConvertTime(Now, Zone).Date;

    /// <summary>The instant of <paramref name="date"/> at <paramref name="time"/> hotel time.</summary>
    public DateTimeOffset At(DateTime date, TimeOnly time)
    {
        var local = DateTime.SpecifyKind(date.Date + time.ToTimeSpan(), DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Zone.GetUtcOffset(local));
    }

    /// <summary>The check-out instant of a stay that ends on <paramref name="checkOutDate"/>.</summary>
    public DateTimeOffset CheckOutInstant(DateTime checkOutDate) =>
        At(checkOutDate, TimeOnly.ParseExact(settings.Value.CheckOutTime, "HH:mm"));
}
