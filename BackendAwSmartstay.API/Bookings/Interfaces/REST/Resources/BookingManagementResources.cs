namespace BackendAwSmartstay.API.Bookings.Interfaces.REST.Resources;

/// <summary>
///     Changes to a booking (US-07 scenario 3). Omitted fields keep their value; at least one is required.
/// </summary>
public record RescheduleBookingResource
{
    /// <summary>New check-in date (yyyy-MM-dd).</summary>
    /// <example>2026-10-02</example>
    public DateTime? CheckInDate { get; init; }

    /// <summary>New check-out date (yyyy-MM-dd).</summary>
    /// <example>2026-10-05</example>
    public DateTime? CheckOutDate { get; init; }

    /// <summary>Another room of the same hotel.</summary>
    /// <example>102</example>
    public int? RoomId { get; init; }
}

/// <summary>A booking as shown in the calendar.</summary>
public record CalendarBookingResource(int Id, string Code, int RoomId, string RoomNumber, string GuestName, string GuestEmail, string? GuestPhone,
    DateOnly CheckInDate, DateOnly CheckOutDate, int Nights, string Status, decimal TotalPrice, DateTimeOffset? PaymentDueAt);

/// <summary>One date of the calendar: ids of the bookings arriving, leaving and staying that night.</summary>
public record CalendarDayResource(DateOnly Date, IReadOnlyList<int> Arrivals, IReadOnlyList<int> Departures, IReadOnlyList<int> InHouse, int OccupiedRooms);

/// <summary>US-07 scenario 1: the active bookings (Pending, Confirmed, CheckedIn) of a period, organized by date.</summary>
public record BookingCalendarResource(int? HotelId, DateOnly From, DateOnly To, IReadOnlyList<CalendarBookingResource> Bookings, IReadOnlyList<CalendarDayResource> Days);

/// <summary>Result of the payment hold job.</summary>
/// <param name="Expired">Pending bookings cancelled because their payment deadline passed.</param>
public record ExpiredBookingsResource(int Expired);
