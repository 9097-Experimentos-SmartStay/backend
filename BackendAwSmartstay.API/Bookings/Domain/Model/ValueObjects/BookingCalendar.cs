using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

/// <summary>One date of the booking calendar.</summary>
/// <param name="Date">The date.</param>
/// <param name="Arrivals">Bookings whose check-in is that date.</param>
/// <param name="Departures">Bookings whose check-out is that date.</param>
/// <param name="InHouse">Bookings that hold a room that night.</param>
public sealed record BookingCalendarDay(DateTime Date, IReadOnlyList<int> Arrivals, IReadOnlyList<int> Departures, IReadOnlyList<int> InHouse);

/// <summary>US-07 scenario 1: the bookings of a period organized by date.</summary>
public sealed record BookingCalendar(int? HotelId, DateRange Window, IReadOnlyList<Booking> Bookings, IReadOnlyList<BookingCalendarDay> Days)
{
    /// <summary>Builds the day-by-day view of <paramref name="bookings"/> for every date of <paramref name="window"/>.</summary>
    public static BookingCalendar Build(int? hotelId, DateRange window, IReadOnlyList<Booking> bookings)
    {
        var days = new List<BookingCalendarDay>(window.Nights);
        for (var date = window.CheckIn; date < window.CheckOut; date = date.AddDays(1))
        {
            var day = date;
            days.Add(new BookingCalendarDay(day,
                bookings.Where(b => b.CheckInDate == day).Select(b => b.Id).ToList(),
                bookings.Where(b => b.CheckOutDate == day).Select(b => b.Id).ToList(),
                bookings.Where(b => b.Dates.Includes(day)).Select(b => b.Id).ToList()));
        }
        return new BookingCalendar(hotelId, window, bookings, days);
    }
}
