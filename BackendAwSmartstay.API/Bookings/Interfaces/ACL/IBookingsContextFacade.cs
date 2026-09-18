namespace BackendAwSmartstay.API.Bookings.Interfaces.ACL;

/// <summary>
///     Snapshot of a booking exposed to other bounded contexts (no domain entity leaks through the ACL).
/// </summary>
/// <param name="BookingId">The booking identifier.</param>
/// <param name="RoomId">The booked room.</param>
/// <param name="CheckInDate">Check-in date.</param>
/// <param name="CheckOutDate">Check-out date.</param>
/// <param name="Nights">Number of nights.</param>
/// <param name="Status">Pending, Confirmed, Cancelled or Completed.</param>
/// <param name="CanBePaid">True while the booking is Pending or Confirmed.</param>
public record BookingSnapshot(
    int BookingId,
    int RoomId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int Nights,
    string Status,
    bool CanBePaid);

/// <summary>
///     Anti-corruption layer facade of the Bookings bounded context.
/// </summary>
public interface IBookingsContextFacade
{
    /// <summary>
    ///     Returns the booking snapshot. When <paramref name="guestUserId"/> is given, the booking is only
    ///     returned if that guest owns it (otherwise null, as if it did not exist).
    /// </summary>
    Task<BookingSnapshot?> FetchBookingAsync(int bookingId, int? guestUserId = null);

    /// <summary>
    ///     Confirms the booking through the Bookings application layer.
    ///     Changes pending in the shared unit of work (e.g. a new payment) are committed together.
    /// </summary>
    /// <exception cref="BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions.EntityNotFoundException">The booking does not exist.</exception>
    Task<bool> ConfirmBookingAsync(int bookingId);

    /// <summary>
    ///     True when the guest has a Confirmed booking of <paramref name="roomId"/> whose dates include
    ///     <paramref name="day"/> (R5: a guest controls the devices of the room they are staying in).
    /// </summary>
    Task<bool> HasCurrentConfirmedStayAsync(int guestUserId, int roomId, DateTime day);
}
