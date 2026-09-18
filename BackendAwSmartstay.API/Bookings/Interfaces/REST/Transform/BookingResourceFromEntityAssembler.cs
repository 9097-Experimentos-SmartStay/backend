using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Transform;

public static class BookingResourceFromEntityAssembler
{
    public static BookingResource ToResourceFromEntity(Booking entity) => new(
        entity.Id,
        entity.Code.Value,
        entity.HotelId,
        entity.RoomId,
        entity.GuestName,
        entity.GuestEmail,
        entity.GuestPhone,
        entity.CheckInDate,
        entity.CheckOutDate,
        entity.Nights,
        entity.PricePerNight,
        entity.TotalPrice,
        entity.Status.ToString(),
        entity.CreatedAt,
        entity.PaymentDueAt,
        entity.ConfirmedAt,
        entity.CancelledAt,
        entity.CancellationReason?.ToString(),
        entity.CheckedInAt,
        entity.GuestProfileId,
        entity.GuestId?.Value);

    public static BookingCalendarResource ToResource(BookingCalendar calendar) => new(
        calendar.HotelId,
        DateOnly.FromDateTime(calendar.Window.CheckIn),
        DateOnly.FromDateTime(calendar.Window.CheckOut),
        calendar.Bookings.Select(b => new CalendarBookingResource(b.Id, b.Code.Value, b.RoomId, b.GuestName, b.GuestEmail,
            b.GuestPhone, DateOnly.FromDateTime(b.CheckInDate), DateOnly.FromDateTime(b.CheckOutDate), b.Nights,
            b.Status.ToString(), b.TotalPrice, b.PaymentDueAt)).ToList(),
        calendar.Days.Select(d => new CalendarDayResource(DateOnly.FromDateTime(d.Date), d.Arrivals, d.Departures, d.InHouse,
            d.InHouse.Count)).ToList());
}
