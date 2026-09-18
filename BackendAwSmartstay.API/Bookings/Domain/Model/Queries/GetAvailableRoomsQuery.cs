using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Queries;

/// <summary>
///     Rooms that can be booked for <paramref name="Dates"/> (US-29 scenario 1): offered for booking (not under
///     maintenance) and without an active booking that shares a night (R1).
/// </summary>
/// <param name="HotelId">Only this hotel, or null for every hotel.</param>
/// <param name="Dates">The stay.</param>
public record GetAvailableRoomsQuery(int? HotelId, DateRange Dates);
