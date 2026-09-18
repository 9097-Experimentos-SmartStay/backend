using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;

namespace BackendAwSmartstay.API.Bookings.Domain.Model.Commands;

/// <summary>US-07 scenario 3: move a booking to other dates and/or another room of the same hotel.</summary>
/// <param name="BookingId">The booking.</param>
/// <param name="Requester">Staff of the booking's hotel (or a chain administrator).</param>
/// <param name="CheckInDate">New check-in date, or null to keep it.</param>
/// <param name="CheckOutDate">New check-out date, or null to keep it.</param>
/// <param name="RoomId">New room, or null to keep it.</param>
public record RescheduleBookingCommand(int BookingId, BookingRequester Requester, DateTime? CheckInDate, DateTime? CheckOutDate, int? RoomId);

/// <summary>Payment hold: cancel the Pending bookings whose payment deadline passed. Run by the scheduler.</summary>
public record ExpireUnpaidBookingsCommand;
